using Domain.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;
using System.Text.Json;

namespace Infrastructure.Messaging;

public class RabbitMqEventPublisher : IEventPublisher
{
    private readonly ResiliencePipeline _retryPolicy;
    private readonly RabbitMqPersistentConnection _persistentConnection;
    private readonly ILogger<RabbitMqEventPublisher> _logger;

    public RabbitMqEventPublisher(RabbitMqPersistentConnection persistentConnection, ILogger<RabbitMqEventPublisher> logger)
    {
        _logger = logger;
        _persistentConnection = persistentConnection;

        _retryPolicy = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = new PredicateBuilder()
                    .Handle<BrokerUnreachableException>()
                    .Handle<SocketException>()
                    .Handle<TimeoutException>()
                    .Handle<IOException>()
                    .Handle<AlreadyClosedException>(),
                OnRetry = args =>
                {
                    _logger.LogWarning("Publish Batch Error. Retrying in {SleepDuration}... (Attempt {AttemptNumber})",
                        args.RetryDelay, args.AttemptNumber);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    public async Task PublishBatchAsync<T>(IEnumerable<T> events, string exchange, string routingKey, CancellationToken cancellationToken = default)
    {
        await _retryPolicy.ExecuteAsync(async (token) =>
        {
            _persistentConnection.EnsureConnected();
            using var channel = _persistentConnection.CreateModel();
            channel.ConfirmSelect();

            RabbitMqTopology.Configure(channel);

            var batch = channel.CreateBasicPublishBatch();
            var properties = channel.CreateBasicProperties();

            properties.Persistent = true;
            properties.DeliveryMode = 2;

            foreach (var @event in events)
            {
                token.ThrowIfCancellationRequested();
                var body = JsonSerializer.SerializeToUtf8Bytes(@event);
                batch.Add(exchange, routingKey, mandatory: false, properties, new ReadOnlyMemory<byte>(body));
            }

            batch.Publish();
            var timeout = TimeSpan.FromSeconds(Math.Max(10, events.Count() / 500));
            channel.WaitForConfirmsOrDie(timeout);
            await Task.CompletedTask;

        }, cancellationToken);
    }
}