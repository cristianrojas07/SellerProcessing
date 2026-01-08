namespace Domain.Abstractions.Messaging;

public interface IEventPublisher
{
    Task PublishBatchAsync<T>(IEnumerable<T> events, string exchange, string routingKey, CancellationToken ct);
}
