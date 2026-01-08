using Application.Abstractions.Messaging;
using Application.Common.Settings;
using Application.DTOs;
using Domain.Abstractions.Messaging;
using Domain.Common;
using Domain.Constants;
using Domain.Entities.Sellers;
using Domain.Events;
using Microsoft.Extensions.Options;
using MiniExcelLibs;

namespace Application.Features.Sellers.Commands.UploadSellers;

public record UploadSellersCommand(Stream FileStream, string FileName) : ICommand;

public class UploadSellersHandler(IEventPublisher eventPublisher,
    IOptions<RabbitMqOptions> options) : ICommandHandler<UploadSellersCommand>
{
    private readonly int _batchSize = options.Value.PublishBatchSize > 0 ? options.Value.PublishBatchSize : 2000;

    public async Task<Result> Handle(UploadSellersCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var rows = command.FileStream.Query<SellerImportDto>();

            if (!rows.Any()) return Result.Failure(Error.InvalidRecords);

            var currentBatch = new List<SellerCreatedEvent>(_batchSize);

            foreach (var row in rows)
            {
                if (string.IsNullOrWhiteSpace(row.Email)) continue;

                var sellerEvent = new SellerCreatedEvent(
                    row.Id, row.FirstName, row.LastName, row.Email,
                    row.PhoneNumber, row.Region, row.IsActive, row.CreatedAt
                );

                currentBatch.Add(sellerEvent);

                if (currentBatch.Count >= _batchSize)
                {
                    await PublishBatchAsync(currentBatch, cancellationToken);

                    currentBatch = new List<SellerCreatedEvent>(_batchSize);
                }
            }

            if (currentBatch.Count > 0)
            {
                await PublishBatchAsync(currentBatch, cancellationToken);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(SellerErrors.ErrorProcessingFile(ex.Message));
        }
    }

    private async Task PublishBatchAsync(List<SellerCreatedEvent> batch, CancellationToken token)
    {
        await eventPublisher.PublishBatchAsync(
            batch,
            RabbitMqSettings.SellerExchange,
            RabbitMqSettings.SellerRoutingKey,
            token
        );
    }
}