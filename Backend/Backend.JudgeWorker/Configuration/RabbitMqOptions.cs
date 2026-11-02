namespace Backend.JudgeWorker.Configuration;

public sealed class RabbitMqOptions
{
    required public string BrokerUri { get; set; }

    required public string ContainerId { get; set; }

    required public string QueueName { get; set; }

    public string? DeadLetterExchange { get; set; }

    required public ushort PrefetchCount { get; set; }
}
