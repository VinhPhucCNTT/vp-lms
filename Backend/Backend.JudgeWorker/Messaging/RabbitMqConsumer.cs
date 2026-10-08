using System.Text.Json;
using Backend.JudgeWorker.Configuration;
using Backend.Judge.Contracts;
using Backend.JudgeWorker.Services;
using Microsoft.Extensions.Options;
using RabbitMQ.AMQP.Client;
using RabbitMQ.AMQP.Client.Impl;

namespace Backend.JudgeWorker.Messaging;

public sealed class RabbitMqConsumer(
    IOptions<RabbitMqOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<RabbitMqConsumer> logger)
    : BackgroundService
{
    private readonly RabbitMqOptions _options =
        options.Value;

    private IEnvironment? _environment;
    private IConnection? _connection;
    private IConsumer? _consumer;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var settings =
            ConnectionSettingsBuilder.Create()
                .Uri(new Uri(_options.BrokerUri))
                .ContainerId($"judge-worker-{Guid.NewGuid():N}")
                .Build();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                    "Connecting to RabbitMQ with Uri {Uri}",
                    _options.BrokerUri);
        }

        _environment =
            AmqpEnvironment.Create(settings);

        _connection =
            await _environment.CreateConnectionAsync();

        // await DeclareTopologyAsync(
        //     _connection,
        //     stoppingToken);

        var management = _connection.Management();

        var queueSpec =
            management
                .Queue(_options.QueueName)
                .Type(QueueType.QUORUM);

        await queueSpec.DeclareAsync();

        _consumer =
            await _connection
                .ConsumerBuilder()
                .Queue(_options.QueueName)
                .InitialCredits(_options.PrefetchCount)
                .MessageHandler(
                    async (context, message) =>
                    {
                        await HandleMessageAsync(
                            context,
                            message,
                            stoppingToken);
                    })
                .BuildAndStartAsync(
                    stoppingToken);

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
        IContext context,
        IMessage message,
        CancellationToken stoppingToken)
    {
        var body =
            message.Body()
            ?? [];

        JudgeSubmissionMessage submission;

        try
        {
            submission =
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

            // Equivalent to BasicNack(requeue: false).
            context.Discard();

            return;
        }

        try
        {
            logger.LogInformation(
                "Received submission {SubmissionId}",
                submission.SubmissionId);

            using var scope =
                scopeFactory.CreateScope();

            var processor =
                scope.ServiceProvider
                    .GetRequiredService<ISubmissionProcessor>();

            await processor.ProcessAsync(
                submission.SubmissionId,
                stoppingToken);

            // Equivalent to BasicAck.
            context.Accept();

            logger.LogInformation(
                "ACKed submission {SubmissionId}",
                submission.SubmissionId);
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            // Do not settle the message.
            //
            // Leaving the delivery unsettled allows it to be
            // redelivered after the consumer/connection closes.
            logger.LogInformation(
                "Worker shutting down while processing submission {SubmissionId}",
                submission.SubmissionId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to process submission {SubmissionId}",
                submission.SubmissionId);

            // Equivalent to BasicNack(requeue: false).
            context.Discard();
        }
    }

    private async Task DeclareTopologyAsync(
        IConnection connection,
        CancellationToken cancellationToken)
    {
        var management = connection.Management();

        // var deadLetterExchange =
        //     management
        //         .Exchange(_options.DeadLetterExchange)
        //         .Type(ExchangeType.DIRECT);
        //
        // await deadLetterExchange.DeclareAsync();
        //
        // var deadLetterQueue =
        //     $"{_options.QueueName}.dead";
        //
        // var deadLetterQueueSpec =
        //     management
        //         .Queue(deadLetterQueue)
        //         .Type(QueueType.QUORUM);
        //
        // await deadLetterQueueSpec.DeclareAsync();
        //
        // var binding =
        //     management
        //         .Binding()
        //         .SourceExchange(deadLetterExchange)
        //         .DestinationQueue(deadLetterQueue)
        //         .Key(_options.QueueName);
        //
        // await binding.BindAsync();

        var queueSpec =
            management
                .Queue(_options.QueueName)
                .Type(QueueType.QUORUM);
                // .DeadLetterExchange(_options.DeadLetterExchange)
                // .DeadLetterRoutingKey(_options.QueueName);

        await queueSpec.DeclareAsync();
    }

    public override async Task StopAsync(
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Stopping judge worker...");

        if (_consumer is not null)
        {
            await _consumer.CloseAsync();
            _consumer.Dispose();
        }

        if (_connection is not null)
        {
            await _connection.CloseAsync();
        }

        if (_environment is not null)
        {
            await _environment.CloseAsync();
        }

        await base.StopAsync(
            cancellationToken);
    }
}
