using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace JudgeWorker;

public class WorkerMain(
    IServiceScopeFactory scopeFactory,
    ILogger<WorkerMain> logger,
    IConfiguration config) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly RabbitMQSettings _settings = config.GetSection("RabbitMQSettings").Get<RabbitMQSettings>()!;

    protected override async Task ExecuteAsync(CancellationToken cs = default)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        using var connection = await factory.CreateConnectionAsync(cancellationToken: cs);
        using var channel = await connection.CreateChannelAsync(cancellationToken: cs);

        await channel.QueueDeclareAsync(
            cancellationToken: cs,
            queue: _settings.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?> { { "x-queue-type", "quorum" } });
        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: cs);

        logger.LogDebug(" Worker started");
        var consumer = new AsyncEventingBasicConsumer(channel);
    }
}
