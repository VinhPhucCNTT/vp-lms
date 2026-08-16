using System.Text;
using System.Text.Json;
using Backend.JudgeWorker.Configuration;
using Backend.JudgeWorker.Contracts;
using Backend.JudgeWorker.Interfaces;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Backend.JudgeWorker.Messaging;

public sealed class RabbitMqConsumer(
    IOptions<RabbitMqOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<RabbitMqConsumer> logger)
    : BackgroundService
{
    private readonly RabbitMqOptions _options =
        options.Value;

    private IConnection? _connection;
    private IChannel? _channel;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,

            AutomaticRecoveryEnabled = true
        };

        logger.LogInformation(
            "Connecting to RabbitMQ at {Host}:{Port}",
            _options.HostName,
            _options.Port);

        _connection =
            await factory.CreateConnectionAsync(
                cancellationToken: stoppingToken);

        _channel =
            await _connection.CreateChannelAsync(
                cancellationToken: stoppingToken);

        await DeclareTopologyAsync(
            _channel,
            stoppingToken);

        await _channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: _options.PrefetchCount,
            global: false,
            cancellationToken: stoppingToken);

        var consumer =
            new AsyncEventingBasicConsumer(
                _channel);

        consumer.ReceivedAsync +=
            async (_, eventArgs) =>
            {
                await HandleMessageAsync(
                    eventArgs,
                    stoppingToken);
            };

        await _channel.BasicConsumeAsync(
            queue: _options.QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        logger.LogInformation(
            "Judge worker is listening on {Queue}",
            _options.QueueName);

        try
        {
            await Task.Delay(
                Timeout.InfiniteTimeSpan,
                stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }
    }

    private async Task HandleMessageAsync(
        BasicDeliverEventArgs eventArgs,
        CancellationToken stoppingToken)
    {
        if (_channel is null)
        {
            return;
        }

        // RabbitMQ owns the delivery buffer, so copy it before
        // doing asynchronous work.
        var body = eventArgs.Body.ToArray();

        JudgeSubmissionMessage message;

        try
        {
            message =
                JsonSerializer.Deserialize<JudgeSubmissionMessage>(
                    body)
                ?? throw new JsonException(
                    "Message deserialized to null.");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Invalid submission message.");

            await _channel.BasicNackAsync(
                deliveryTag: eventArgs.DeliveryTag,
                multiple: false,
                requeue: false,
                cancellationToken: stoppingToken);

            return;
        }

        try
        {
            logger.LogInformation(
                "Received submission {SubmissionId}",
                message.SubmissionId);

            using var scope =
                scopeFactory.CreateScope();

            var processor =
                scope.ServiceProvider
                    .GetRequiredService<ISubmissionProcessor>();

            await processor.ProcessAsync(
                message.SubmissionId,
                stoppingToken);

            await _channel.BasicAckAsync(
                deliveryTag: eventArgs.DeliveryTag,
                multiple: false,
                cancellationToken: stoppingToken);

            logger.LogInformation(
                "ACKed submission {SubmissionId}",
                message.SubmissionId);
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            // Do not ACK. RabbitMQ can redeliver after shutdown.
            logger.LogInformation(
                "Worker shutting down while processing submission {SubmissionId}",
                message.SubmissionId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to process submission {SubmissionId}",
                message.SubmissionId);

            await _channel.BasicNackAsync(
                deliveryTag: eventArgs.DeliveryTag,
                multiple: false,

                // Don't endlessly requeue a poison message.
                // Configure a dead-letter queue below.
                requeue: false,

                cancellationToken: stoppingToken);
        }
    }

    private async Task DeclareTopologyAsync(
        IChannel channel,
        CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(
            exchange: _options.DeadLetterExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var deadLetterQueue =
            $"{_options.QueueName}.dead";

        await channel.QueueDeclareAsync(
            queue: deadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: deadLetterQueue,
            exchange: _options.DeadLetterExchange,
            routingKey: _options.QueueName,
            cancellationToken: cancellationToken);

        var arguments =
            new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] =
                    _options.DeadLetterExchange,

                ["x-dead-letter-routing-key"] =
                    _options.QueueName
            };

        await channel.QueueDeclareAsync(
            queue: _options.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: arguments,
            cancellationToken: cancellationToken);
    }

    public override async Task StopAsync(
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Stopping judge worker...");

        if (_channel is not null)
        {
            await _channel.CloseAsync(
                cancellationToken);
        }

        if (_connection is not null)
        {
            await _connection.CloseAsync(
                cancellationToken);
        }

        await base.StopAsync(
            cancellationToken);
    }
}
