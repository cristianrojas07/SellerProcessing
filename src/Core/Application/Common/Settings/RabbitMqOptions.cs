namespace Application.Common.Settings;

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";
    public string HostName { get; set; } = "localhost";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public int PublishBatchSize { get; set; } = 5000;
    public int ConsumerBatchSize { get; set; } = 1000;
    public int MaxWaitTimeMs { get; set; } = 3000;
}