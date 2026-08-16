namespace Backend.JudgeWorker.Configuration;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; set; } = "localhost";

    public int Port { get; set; } = 5672;

    public string UserName { get; set; } = "guest";

    public string Password { get; set; } = "guest";

    public string QueueName { get; set; } = "judge.submissions";

    public string DeadLetterExchange { get; set; } = "judge.dlx";

    public ushort PrefetchCount { get; set; } = 1;
}
