using System.Text;
using System.Text.Json;
using Backend.Api.Core.Common;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Backend.Api.Services.Judge;

public sealed record JudgeSubmissionMessage(
    long SubmissionId);

public sealed class RabbitMqJudgePublisher(
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqJudgePublisher> logger)
{
    private readonly RabbitMqOptions _options =
        options.Value;

    public async Task PublishAsync(
        long submissionId,
        CancellationToken ct)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            AutomaticRecoveryEnabled = true
        };

        await using var connection =
            await factory.CreateConnectionAsync(
                cancellationToken: ct);

        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken: ct);

        await channel.QueueDeclareAsync(
            queue: _options.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: ct);

        var message =
            new JudgeSubmissionMessage(submissionId);

        var body = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(message));

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: _options.QueueName,
            body: body,
            cancellationToken: ct);

        logger.LogInformation(
            "Published submission {SubmissionId} to {Queue}",
            submissionId,
            _options.QueueName);
    }
}
