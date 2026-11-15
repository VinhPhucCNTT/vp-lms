using System.Text;
using System.Text.Json;
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

public interface IProblemSubmissionService
{
    public Task<ProblemSubmissionResponse?>
        Get(long submissionId, CancellationToken ct = default);

    public Task<Result<ProblemSubmissionResponse>>
        Send(long activityId,
            ProblemSubmissionRequest request,
            CancellationToken ct = default);
}

public sealed class ProblemSubmissionService(
    ILogger<ProblemSubmissionService> logger,
    IDbContextFactory<AppDbContext> dbFactory,
    CurrentUserService currentUserService,
    IMapper mapper,
    IOptions<RabbitMqOptions> options) : IProblemSubmissionService
{
    private readonly ILogger<ProblemSubmissionService> _logger = logger;
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly CurrentUserService _currentUserService = currentUserService;
    private readonly IMapper _mapper = mapper;
    private readonly RabbitMqOptions _options = options.Value;

    private IConnection? _connection;
    private IPublisher? _publisher;
    private IEnvironment? _environment;

    public async Task<ProblemSubmissionResponse?> Get(long submissionId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.ProblemSubmissions
            .AsNoTracking()
            .Where(s => s.Id == submissionId)
            .Select(s => _mapper.Map<ProblemSubmissionResponse>(s))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Result<ProblemSubmissionResponse>> Send(long activityId, ProblemSubmissionRequest request, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var problem = await db.CodingProblems.FirstOrDefaultAsync(p => p.ActivityId == activityId, ct);
        if (problem is null) return Result<ProblemSubmissionResponse>.Failure(ResultError.NotFound, "Problem not found.");

        // TODO: See if this needs to be changed?
        var existing = problem.Submissions
            .Where(s =>
                s.Status == SubmissionStatus.Queued &&
                s.Status == SubmissionStatus.Running);
        if (existing.Any())
            return Result<ProblemSubmissionResponse>.Failure(ResultError.Forbidden, "Another submission is being processed.");

        var profile = db.CompilerProfiles.FirstOrDefaultAsync(p => p.ProblemId == problem.Id && p.Compiler.Language == request.Language, ct);
        if (profile is null)
            return Result<ProblemSubmissionResponse>.Failure(ResultError.BadRequest, $"Profile not defined for this language ({request.Language}).");

        if (await InitializeConnectionAsync())
            return Result<ProblemSubmissionResponse>.Failure(ResultError.Internal, "");

        try
        {
            ProblemSubmission submission = new()
            {
                ProblemId = problem.Id,
                UserId = _currentUserService.UserId,
                ProfileId = profile.Id,

                OrderIndex = db.ProblemSubmissions.Where(s => s.ProblemId == problem.Id && s.UserId == _currentUserService.UserId).Count(),
                SubmittedCode = request.SourceCode,
                Status = SubmissionStatus.Queued
            };
            db.ProblemSubmissions.Add(submission);
            await db.SaveChangesAsync(ct);

            var message = JsonSerializer.Serialize(new JudgeSubmissionMessage(submission.Id));
            var amqpMessage = new AmqpMessage(Encoding.UTF8.GetBytes(message));
            PublishResult pr = await _publisher!.PublishAsync(amqpMessage, ct);
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
                        s.Status != SubmissionStatus.Running, ct);
                if (hasResult)
                    break;

                await Task.Delay(500, ct);
                pollingCount--;
            }

            var result = await db.ProblemSubmissions.FirstOrDefaultAsync(s => s.Id == submission.Id, ct);
            System.Diagnostics.Debug.Assert(result is not null);

            return Result.Success(_mapper.Map<ProblemSubmissionResponse>(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occured when publishing.");
            return Result<ProblemSubmissionResponse>.Failure(ResultError.Internal, "");
        }
        finally
        {
            if (_connection is not null)
                await _connection.CloseAsync();
            if (_publisher is not null)
                await _publisher.CloseAsync();
            if (_environment is not null)
                await _environment.CloseAsync();
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

            _environment = AmqpEnvironment.Create(settings);
            _connection = await _environment.CreateConnectionAsync();

            IManagement management = _connection.Management();
            IQueueSpecification queueSpec = management.Queue(_options.QueueName).Type(QueueType.QUORUM);
            await queueSpec.DeclareAsync();

            _publisher = await _connection.PublisherBuilder()
                .Queue(_options.QueueName)
                .BuildAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occured when creating connection.");
            if (_connection is not null)
                await _connection.CloseAsync();
            if (_publisher is not null)
                await _publisher.CloseAsync();
            return true;
        }

        return false;
    }
}
