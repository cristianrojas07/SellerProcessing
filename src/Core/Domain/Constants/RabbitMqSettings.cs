namespace Domain.Constants;

public static class RabbitMqSettings
{
    // Queue Primary
    public const string SellerExchange = "seller-exchange";
    public const string SellerCreationQueue = "seller-creation-queue";
    public const string SellerRoutingKey = "seller-creation-queue";

    // DLQ
    public const string SellerDlExchange = "seller-exchange.dlx";
    public const string SellerDlQueue = "seller-creation-queue.dlq";
    public const string SellerDlRoutingKey = "seller-creation-queue.dlq";
}

public static class RabbitMqHeaders
{
    // Argumentos estándar de RabbitMQ (x-arguments)
    public const string DeadLetterExchange = "x-dead-letter-exchange";
    public const string DeadLetterRoutingKey = "x-dead-letter-routing-key";
    public const string MessageTtl = "x-message-ttl"; // Por si usas expiración
}