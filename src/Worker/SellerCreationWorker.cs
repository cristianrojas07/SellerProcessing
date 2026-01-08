using Application.Common.Settings;
using Domain.Constants;
using Domain.Entities.Sellers;
using Domain.Events;
using EFCore.BulkExtensions;
using Infrastructure.Messaging;
using Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;

namespace Worker;

public class SellerCreationWorker : RabbitMqConsumer<SellerCreatedEvent>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ResiliencePipeline _retryPolicy;
    private readonly ILogger<SellerCreationWorker> _logger;
    private readonly IOptions<RabbitMqOptions> _options;

    public SellerCreationWorker(
        RabbitMqPersistentConnection persistentConnection,
        ILogger<SellerCreationWorker> logger,
        IServiceProvider serviceProvider,
        IOptions<RabbitMqOptions> options)
        : base(persistentConnection, logger, RabbitMqSettings.SellerCreationQueue, options)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _options = options;

        _retryPolicy = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(2),
                ShouldHandle = new PredicateBuilder()
                    .Handle<TimeoutException>()
                    .Handle<SqlException>(ex => ex.Number is -2),
                OnRetry = args =>
                {
                    _logger.LogWarning("Infra Error. Retrying batch... (Attempt {AttemptNumber})", args.AttemptNumber);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    protected override async Task ProcessBatchAsync(List<BatchItem<SellerCreatedEvent>> batch, IModel channel, CancellationToken token)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var incomingEmails = batch
            .Select(x => x.Message.Email)
            .Distinct()
            .ToList();

        var existingEmails = await dbContext.Sellers
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(s => incomingEmails.Contains(s.Email))
            .Select(s => s.Email)
            .ToListAsync(token);

        var existingSet = new HashSet<string>(existingEmails);
        var sellersToInsert = new List<Seller>();
        var deliveryTagsProcessed = new List<ulong>();

        foreach (var item in batch)
        {
            var msg = item.Message;
            deliveryTagsProcessed.Add(item.DeliveryTag);

            // Idempotency check
            if (existingSet.Contains(msg.Email))
            {
                _logger.LogInformation("Skipping existing seller: {Email}", msg.Email);
                continue;
            }

            var sellerResult = Seller.Import(
                msg.Id, msg.FirstName, msg.LastName, msg.Email,
                msg.PhoneNumber, msg.Region, msg.IsActive, msg.CreatedAt);

            if (sellerResult.IsFailure)
            {
                _logger.LogWarning("Domain validation failed for {Email}: {Error}", msg.Email, sellerResult.Error);
                continue;
            }

            sellersToInsert.Add(sellerResult.Value!);
        }

        if (!sellersToInsert.Any() && deliveryTagsProcessed.Any())
        {
            channel.BasicAck(deliveryTagsProcessed.Last(), multiple: true);

            _logger.LogInformation("Prefetch buffer cleared: {Count} duplicates skipped instantly.",
                deliveryTagsProcessed.Count);

            return;
        }

        try
        {
            await _retryPolicy.ExecuteAsync(async ct =>
            {
                await dbContext.BulkInsertAsync(
                        sellersToInsert,
                        bulkConfig: new BulkConfig
                        {
                            BatchSize = _options.Value.ConsumerBatchSize,
                            BulkCopyTimeout = 180,
                            SetOutputIdentity = false,
                            PreserveInsertOrder = false,
                            WithHoldlock = true,
                        }, cancellationToken: ct);
            }, token);

            _logger.LogInformation("Inserted {Count} new sellers successfully.", sellersToInsert.Count);

            foreach (var tag in deliveryTagsProcessed)
            {
                channel.BasicAck(tag, multiple: false);
            }
        }
        catch (Exception ex) when (ex is DbUpdateException or SqlException { Number: 2601 or 2627 })
        {
            _logger.LogWarning("Duplicate detected during bulk insert. Falling back to granular processing.");

            dbContext.ChangeTracker.Clear();
            await HandleConflictFallback(batch, dbContext, channel, token);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Infrastructure failure. Requeueing batch.");
            foreach (var tag in deliveryTagsProcessed) channel.BasicNack(tag, false, true);
        }
    }

    private async Task HandleConflictFallback(List<BatchItem<SellerCreatedEvent>> batch, AppDbContext dbContext, IModel channel, CancellationToken token)
    {
        foreach (var item in batch)
        {
            try
            {
                var msg = item.Message;
                var existing = await dbContext.Sellers.FirstOrDefaultAsync(s => s.Email == msg.Email, token);

                if (existing == null)
                {
                    var result = Seller.Import(msg.Id, msg.FirstName, msg.LastName, msg.Email, msg.PhoneNumber, msg.Region, msg.IsActive, msg.CreatedAt);
                    if (result.IsSuccess) dbContext.Sellers.Add(result.Value!);
                }
                else
                {
                    existing.Update(msg.FirstName, msg.LastName, msg.Email, msg.PhoneNumber, msg.Region, msg.IsActive);
                }

                await dbContext.SaveChangesAsync(token);
                channel.BasicAck(item.DeliveryTag, false);
            }
            catch (Exception)
            {
                channel.BasicAck(item.DeliveryTag, false);
            }
            finally
            {
                dbContext.ChangeTracker.Clear();
            }
        }
    }
}