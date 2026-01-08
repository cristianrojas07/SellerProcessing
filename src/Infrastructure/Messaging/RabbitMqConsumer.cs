using Application.Common.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace Infrastructure.Messaging;

public record BatchItem<T>(T Message, ulong DeliveryTag);

public abstract class RabbitMqConsumer<T> : BackgroundService
{
    private readonly RabbitMqPersistentConnection _persistentConnection;
    private readonly ILogger<RabbitMqConsumer<T>> _logger;
    private readonly string _queueName;
    private IModel? _consumerChannel;
    private readonly RabbitMqOptions _options;
    private readonly Channel<BatchItem<T>> _channel;

    protected RabbitMqConsumer(
        RabbitMqPersistentConnection persistentConnection,
        ILogger<RabbitMqConsumer<T>> logger,
        string queueName,
        IOptions<RabbitMqOptions> options)
    {
        _persistentConnection = persistentConnection;
        _logger = logger;
        _queueName = queueName;
        _options = options.Value;

        int capacity = _options.ConsumerBatchSize * 10;

        _channel = Channel.CreateBounded<BatchItem<T>>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        });
    }

    protected abstract Task ProcessBatchAsync(List<BatchItem<T>> batch, IModel channel, CancellationToken token);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var consumerTask = Task.Run(async () =>
        {
            var policyRetry = new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = int.MaxValue,
                Delay = TimeSpan.FromSeconds(2),
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = new PredicateBuilder()
                    .Handle<InvalidOperationException>()
                    .Handle<BrokerUnreachableException>()
                    .Handle<SocketException>(),
                OnRetry = args =>
                {
                    _logger.LogWarning($"Connection Error. Retrying... {args.Outcome.Exception?.Message}");
                    return ValueTask.CompletedTask;
                }
            }).Build();

            await policyRetry.ExecuteAsync(async token =>
            {
                _consumerChannel = CreateConsumerChannel();
                var consumer = new AsyncEventingBasicConsumer(_consumerChannel);

                consumer.Received += async (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.Span;
                        var message = Encoding.UTF8.GetString(body);
                        var eventData = JsonSerializer.Deserialize<T>(message);

                        if (eventData != null)
                        {
                            await _channel.Writer.WriteAsync(new BatchItem<T>(eventData, ea.DeliveryTag), token);
                        }
                        else
                        {
                            _logger.LogWarning("Message was null. DLQ.");
                            _consumerChannel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "JSON Malformed. DLQ.");
                        _consumerChannel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Deserialization error. Nack.");
                        _consumerChannel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                    }
                };

                _consumerChannel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);

                await Task.Delay(Timeout.Infinite, token);
            }, cancellationToken);
        }, cancellationToken);

        var processorTask = ProcessBatchesLoop(cancellationToken);

        await Task.WhenAll(consumerTask, processorTask);
    }

    private async Task ProcessBatchesLoop(CancellationToken token)
    {
        var batch = new List<BatchItem<T>>(_options.ConsumerBatchSize);

        while (!token.IsCancellationRequested)
        {
            var cts = new CancellationTokenSource(_options.MaxWaitTimeMs);
            try
            {
                while (batch.Count < _options.ConsumerBatchSize && await _channel.Reader.WaitToReadAsync(cts.Token))
                {
                    if (_channel.Reader.TryRead(out var item))
                    {
                        batch.Add(item);
                    }
                }
            }
            catch (OperationCanceledException) { }

            if (batch.Count > 0)
            {
                if (_consumerChannel != null && _consumerChannel.IsOpen)
                {
                    await ProcessBatchAsync(batch, _consumerChannel, token);
                }
                batch.Clear();
            }
        }
    }

    private IModel CreateConsumerChannel()
    {
        _persistentConnection.EnsureConnected();
        var channel = _persistentConnection.CreateModel();
        RabbitMqTopology.Configure(channel);
        channel.BasicQos(prefetchSize: 0, prefetchCount: (ushort)(_options.ConsumerBatchSize * 3), global: false);
        return channel;
    }

    public override void Dispose()
    {
        _consumerChannel?.Dispose();
        base.Dispose();
    }
}