namespace Backend.Api.Core.Common;

public sealed class RabbitMqOptions
{
    public string BrokerUri { get; set; } = "";

    public string ContainerId { get; set; } = "";

    public string QueueName { get; set; } = "";

    public string DeadLetterExchange { get; set; } = "";

    public ushort PrefetchCount { get; set; } = 1;
}
