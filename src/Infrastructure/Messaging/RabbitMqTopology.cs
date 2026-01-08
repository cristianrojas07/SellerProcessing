using Domain.Constants;
using RabbitMQ.Client;

namespace Infrastructure.Messaging;

public static class RabbitMqTopology
{
    public static void Configure(IModel channel)
    {
        // DLQ
        channel.ExchangeDeclare(
            exchange: RabbitMqSettings.SellerDlExchange,
            type: ExchangeType.Direct,
            durable: true);

        channel.QueueDeclare(
            queue: RabbitMqSettings.SellerDlQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        channel.QueueBind(
            queue: RabbitMqSettings.SellerDlQueue,
            exchange: RabbitMqSettings.SellerDlExchange,
            routingKey: RabbitMqSettings.SellerDlRoutingKey);

        // Main
        channel.ExchangeDeclare(
            exchange: RabbitMqSettings.SellerExchange,
            type: ExchangeType.Direct,
            durable: true);

        var args = new Dictionary<string, object>
        {
            { RabbitMqHeaders.DeadLetterExchange, RabbitMqSettings.SellerDlExchange },
            { RabbitMqHeaders.DeadLetterRoutingKey, RabbitMqSettings.SellerDlRoutingKey }
        };

        channel.QueueDeclare(
            queue: RabbitMqSettings.SellerCreationQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: args);

        channel.QueueBind(
            queue: RabbitMqSettings.SellerCreationQueue,
            exchange: RabbitMqSettings.SellerExchange,
            routingKey: RabbitMqSettings.SellerRoutingKey);
    }
}
