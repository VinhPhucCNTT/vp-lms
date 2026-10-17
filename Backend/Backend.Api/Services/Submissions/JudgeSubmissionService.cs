using System.Text;
using AutoMapper;
using Backend.Api.Core.Common;
using Backend.Api.Core.Types;
using Backend.Api.Services.Common;
using Backend.Judge.Contracts;
using Backend.Persistence.Data;
using Backend.Persistence.Entities.Judge;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.AMQP.Client;
using RabbitMQ.AMQP.Client.Impl;

namespace Backend.Api.Services.Submissions;

public interface IJudgeSubmissionService
{
    public Task<JudgeResponse?>
        Get(long submissionId, CancellationToken ct = default);

    public Task<Result<JudgeResponse>>
        Send(long resourceId,
            JudgeTestRequest request,
            CancellationToken ct = default);
}

public sealed class JudgeSubmissionService(
    ILogger<JudgeSubmissionService> logger,
    IDbContextFactory<AppDbContext> dbFactory,
    CurrentUserService currentUserService,
    IMapper mapper,
    IOptions<RabbitMqOptions> options) : IJudgeSubmissionService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly CurrentUserService _currentUserService = currentUserService;
    private readonly IMapper _mapper = mapper;
    private readonly RabbitMqOptions _options = options.Value;

    private IConnection? _connection;
    private IPublisher? _publisher;

    public async Task<JudgeResponse?> Get(long submissionId, CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.ProblemSubmissions
            .AsNoTracking()
            .Where(s => s.Id == submissionId)
            .Select(s => _mapper.Map<JudgeResponse>(s))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Result<JudgeResponse>> Send(long resourceId, JudgeTestRequest request, CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);

        var problem = await db.CodingProblems.FirstOrDefaultAsync(p => p.ResourceId == resourceId);
        if (problem is null) return Result<JudgeResponse>.Failure(ErrorType.NotFound, "Problem not found.");

        // TODO: See if this needs to be changed?
        var existing = problem.Submissions
            .Where(s =>
                s.Status == SubmissionStatus.Queued &&
                s.Status == SubmissionStatus.Running);
        if (existing.Any())
            return Result<JudgeResponse>.Failure(ErrorType.Forbidden, "Another submission is being processed.");

        if (await InitializeConnectionAsync())
            return Result<JudgeResponse>.Failure(ErrorType.Internal, "");

        try
        {
            ProblemSubmission submission = new ProblemSubmission
            {
                OrderIndex = problem.Submissions.Where(s => s.UserId == _currentUserService.UserId).Count() - 1,
                SubmittedCode = request.SourceCode,
                Status = SubmissionStatus.Queued
            };
            db.ProblemSubmissions.Add(submission);
            await db.SaveChangesAsync(ct);

            var message = new JudgeSubmissionMessage(submission.Id);
            var amqpMessage = new AmqpMessage(Encoding.UTF8.GetBytes(message.ToString()));
            PublishResult pr = await _publisher!.PublishAsync(amqpMessage);
            if (pr.Outcome.State != OutcomeState.Accepted)
                throw new Exception($"Unexpected amqp publish outcome: {pr.Outcome.State}.");

            int pollingCount = 10;
            bool hasResult = false;
            while (pollingCount > 0)
            {
                hasResult = await db.ProblemSubmissions
                    .Where(s => s.Id == submission.Id)
                    .AnyAsync(s =>
                        s.Status != SubmissionStatus.Queued ||
                        s.Status != SubmissionStatus.Running);
                if (hasResult == true)
                    break;

                await Task.Delay(500);
                pollingCount--;
            }

            var result = await db.ProblemSubmissions.FirstOrDefaultAsync(s => s.Id == submission.Id);
            System.Diagnostics.Debug.Assert(result is not null);

            return Result<JudgeResponse>.Success(_mapper.Map<JudgeResponse>(result));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occured when publishing.");
            return Result<JudgeResponse>.Failure(ErrorType.Internal, "");
        }
        finally
        {
            if (_connection is not null)
                await _connection.CloseAsync();
            if (_publisher is not null)
                await _publisher.CloseAsync();
        }
    }

    private async Task<bool> InitializeConnectionAsync()
    {
        try
        {
            var settings = ConnectionSettingsBuilder.Create()
                .Uri(new Uri(_options.BrokerUri))
                .ContainerId(_options.ContainerId)
                .Build();

            IEnvironment environment = AmqpEnvironment.Create(settings);
            _connection = await environment.CreateConnectionAsync();

            IManagement management = _connection.Management();
            IQueueSpecification queueSpec = management.Queue(_options.QueueName).Type(QueueType.QUORUM);
            await queueSpec.DeclareAsync();

            _publisher = await _connection.PublisherBuilder()
                .Queue(_options.QueueName)
                .BuildAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occured when creating connection.");
            return true;
        }
        finally
        {
            if (_connection is not null)
                await _connection.CloseAsync();
            if (_publisher is not null)
                await _publisher.CloseAsync();
        }

        return false;
    }
}
