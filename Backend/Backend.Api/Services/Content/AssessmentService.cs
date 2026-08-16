using Backend.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Backend.Api.Core.Types;
using Backend.Persistence.Entities.Courses;
using Backend.Persistence.Entities.Content;
using AutoMapper;
using Backend.Api.Services.Courses;
using Backend.Api.Core.Common;
using Backend.Api.Services.Common;
using Sqids;

namespace Backend.Api.Services.Content;

public class AssessmentService(
    IDbContextFactory<AppDbContext> dbFactory,
    IMapper mapper,
    CurrentUserService currentUserService,
    SqidsEncoder<long> sqidsEncoder)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly IMapper _mapper = mapper;
    private readonly CurrentUserService _currentUserService = currentUserService;
    private readonly SqidsEncoder<long> _sqidsEncoder = sqidsEncoder;

    public async Task<Assessment?> GetAsync(long resourceId, CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.Assessments
            .Where(a => a.ResourceId == resourceId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<AssessmentResponse?> GetDtoByIdAsync(long resourceId, CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        var userId = _currentUserService.UserId;
        var assessment = await db.Assessments
            .AsNoTracking()
            .Where(a => a.ResourceId == resourceId)
            .Where(a => a.Resource.IsPublished)
            .Where(a => db.Enrollments.Any(e =>
                e.CourseId == a.Resource.Module.CourseId &&
                e.UserId == userId))
            .Include(a => a.Resource)
            .FirstOrDefaultAsync(ct);

        return assessment is null
            ? null
            : new AssessmentResponse(
                _mapper.Map<ResourceDetailResponse>(assessment.Resource),
                ToInfo(assessment));
    }

    public async Task<List<AssessmentListResponse>> QueryAsync(CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        var userId = _currentUserService.UserId;
        var assessments = await db.Assessments
            .AsNoTracking()
            .Where(a => a.Resource.IsPublished)
            .Where(a => db.Enrollments.Any(e =>
                e.CourseId == a.Resource.Module.CourseId &&
                e.UserId == userId))
            .Include(x => x.Resource)
            .ToListAsync(ct);

        var result = new List<AssessmentListResponse>(assessments.Count);
        foreach (var assessment in assessments)
        {
            var questionCount = await db.AssessmentQuestions
                .CountAsync(x => x.AssessmentId == assessment.Id, ct);
            var attempts = await db.AssessmentAttempts
                .Where(x => x.AssessmentId == assessment.Id && x.StudentId == userId)
                .ToListAsync(ct);
            var bestScore = attempts
                .Where(x => x.TotalScore.HasValue)
                .Select(x => x.TotalScore)
                .Max();
            var bestMaxScore = await db.AssessmentQuestions
                .Where(x => x.AssessmentId == assessment.Id)
                .SumAsync(x => x.Points, ct);

            var latestAttempt = attempts
                .OrderByDescending(x => x.AttemptNumber)
                .FirstOrDefault();

            result.Add(new AssessmentListResponse(
                _mapper.Map<ResourceResponse>(assessment.Resource),
                ToInfo(assessment),
                questionCount,
                attempts.Count,
                bestScore,
                bestMaxScore,
                attempts
                    .OrderByDescending(x => x.AttemptNumber)
                    .Select(x => (string?)x.Status.ToString())
                    .FirstOrDefault(),
                latestAttempt is null ? null : _sqidsEncoder.Encode(latestAttempt.Id)));
        }

        return result;
    }

    private static AssessmentInfo ToInfo(Assessment assessment) => new(
        assessment.Description,
        assessment.TimeLimitMinutes > 0
            ? (int)Math.Ceiling(assessment.TimeLimitMinutes)
            : null,
        assessment.MaxAttempts,
        assessment.AvailableFrom,
        assessment.AvailableUntil,
        assessment.ShowResults);

    public async Task<AssessmentResponse> CreateAsync(long moduleId, AssessmentRequest request, CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);

        var resource = await ResourceService.CreateResourceAsync(db, moduleId, request.ResourceInfo, ResourceType.Assessment, ct);
        var assessment = new Assessment
        {
            ResourceId = resource.Id,
            Description = request.Info.Description,
            TimeLimitMinutes = request.Info.TimeLimitMinutes ?? 0,
            MaxAttempts = request.Info.MaxAttempts,
            AvailableFrom = request.Info.AvailableFrom,
            AvailableUntil = request.Info.AvailableUntil,
            ShowResults = request.Info.ShowResults,
        };

        db.Assessments.Add(assessment);
        await db.SaveChangesAsync(ct);
        return new AssessmentResponse(
            _mapper.Map<ResourceDetailResponse>(resource),
            _mapper.Map<AssessmentInfo>(assessment)
        );
    }

    public async Task<AssessmentResponse?> UpdateAsync(long assessmentId, AssessmentRequest request, CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        var assessment = await db.Assessments.FirstOrDefaultAsync(a => a.Id == assessmentId, ct);
        if (assessment is null)
            return null;

        var resource = await ResourceService.UpdateResourceAsync(db, assessment.ResourceId, request.ResourceInfo);

        assessment.Description = request.Info.Description;
        assessment.TimeLimitMinutes = request.Info.TimeLimitMinutes ?? 0;
        assessment.MaxAttempts = request.Info.MaxAttempts;
        assessment.AvailableFrom = request.Info.AvailableFrom;
        assessment.AvailableUntil = request.Info.AvailableUntil;
        assessment.ShowResults = request.Info.ShowResults;

        db.Assessments.Update(assessment);
        await db.SaveChangesAsync(ct);
        return new AssessmentResponse(
            _mapper.Map<ResourceDetailResponse>(resource),
            _mapper.Map<AssessmentInfo>(assessment)
        );
    }

    // public async Task<List<QuestionResponse>?> SetQuestionsAsync(long assessmentId, List<QuestionRequest> requests)
    // {
    //     using var db = await _dbFactory.CreateDbContextAsync();
    //     var assessment = await db.Assessments.FirstOrDefaultAsync(a => a.Id == assessmentId);
    //     if (assessment is null)
    //         return null;
    //
    //     List<QuestionResponse> questions = [];
    //     foreach (var request in requests)
    //     {
    //         long questionId = _sqidsEncoder.Decode(request.Id).SingleOrDefault();
    //         AssessmentQuestion? question;
    //
    //         if (questionId == default)
    //         {
    //             question = new AssessmentQuestion
    //             {
    //                 AssessmentId = assessment.Id,
    //                 QuestionType = request.QuestionType,
    //                 QuestionTextMarkdown = request.QuestionTextMarkdown,
    //                 Points = request.Points,
    //                 OrderIndex = request.OrderIndex,
    //                 QuestionDataJson = request.QuestionDataJson
    //             };
    //             db.AssessmentQuestions.Add(question);
    //         }
    //         else
    //         {
    //             question = await db.AssessmentQuestions.FirstOrDefaultAsync(q => q.Id == questionId);
    //             if (question is null)
    //                 continue;
    //
    //             question.AssessmentId = assessment.Id;
    //             question.QuestionType = request.QuestionType;
    //             question.QuestionTextMarkdown = request.QuestionTextMarkdown;
    //             question.Points = request.Points;
    //             question.OrderIndex = request.OrderIndex;
    //             question.QuestionDataJson = request.QuestionDataJson;
    //
    //             db.AssessmentQuestions.Update(question);
    //         }
    //
    //         questions.Add(new QuestionResponse(
    //             _sqidsEncoder.Encode(question.Id),
    //             question.QuestionType,
    //             question.QuestionTextMarkdown,
    //             question.Points,
    //             question.OrderIndex,
    //             question.QuestionDataJson
    //         ));
    //     }
    //
    //     await db.SaveChangesAsync();
    //     return questions;
    // }
    //
    // public async Task<AssessmentAttemptResponse?> GetLatestAttemptAsync(long assessmentId)
    // {
    //     using var db = await _dbFactory.CreateDbContextAsync();
    //     var currentUserId = _currentUserService.UserId;
    //     return await db.AssessmentAttempts
    //         .AsNoTracking()
    //         .Where(a => a.AssessmentId == assessmentId && a.UserId == currentUserId)
    //         .OrderByDescending(a => a.AttemptNumber)
    //         .Select(a => new AssessmentAttemptResponse(
    //             _sqidsEncoder.Encode(a.Id),
    //             a.StartedAt,
    //             a.SubmittedAt,
    //             a.TotalScore,
    //             a.IsPassed,
    //             a.AttemptNumber
    //         )).FirstOrDefaultAsync();
    // }
    //
    // public async Task<List<AssessmentAttemptResponse>> GetAttemptsAsync(long assessmentId)
    // {
    //     using var db = await _dbFactory.CreateDbContextAsync();
    //     var currentUserId = _currentUserService.UserId;
    //     return await db.AssessmentAttempts
    //         .AsNoTracking()
    //         .Where(a => a.AssessmentId == assessmentId && a.UserId == currentUserId)
    //         .OrderByDescending(a => a.AttemptNumber)
    //         .Select(a => new AssessmentAttemptResponse(
    //             _sqidsEncoder.Encode(a.Id),
    //             a.StartedAt,
    //             a.SubmittedAt,
    //             a.TotalScore,
    //             a.IsPassed,
    //             a.AttemptNumber
    //         )).ToListAsync();
    // }

    // PLACEHOLDER
    // public async Task<AssessmentAttemptResponse?> StartAttemptAsync(long assessmentId)
    // {
    //     using var db = await _dbFactory.CreateDbContextAsync();
    //     var assessment = await db.Assessments.FirstOrDefaultAsync(a => a.Id == assessmentId);
    //     if (assessment is null)
    //         return null;
    //
    //     // TODO: Figure out what to do if there is multiple unfinished attempts (bug)
    //     // Frontend warns?
    //     var currentUserId = _currentUserService.UserId;
    //     var previousAttempts = db.AssessmentAttempts
    //         .Where(a => a.AssessmentId == assessment.Id && a.UserId == currentUserId)
    //         .OrderByDescending(a => a.AttemptNumber);
    //     var previousAttempt = await previousAttempts.FirstOrDefaultAsync();
    //     if (previousAttempt is not null && previousAttempt.IsPassed is null)
    //         return null; // There is an ongoing attempt, frontend should prevent this
    //
    //     var attempt = new AssessmentAttempt
    //     {
    //         AssessmentId = assessmentId,
    //         UserId = currentUserId,
    //         AttemptNumber = await previousAttempts.CountAsync() + 1
    //     };
    //     db.AssessmentAttempts.Add(attempt);
    //     await db.SaveChangesAsync();
    //     return new AssessmentAttemptResponse(
    //         _sqidsEncoder.Encode(attempt.Id),
    //         attempt.StartedAt,
    //         null,
    //         null,
    //         null,
    //         attempt.AttemptNumber
    //     );
    // }

    // public async Task<AssessmentAttemptResponse?> SubmitAttemptAsync(long assessmentId, AssessmentAttemptResponse request) { }

    // public async Task GradeAttemptAsync() { }

    // public async Task GetAssessmentStatsAsync() { }

    public async Task<Result<bool>> SetPublishStatusAsync(long resourceId, bool isPublished, CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        var resource = await db.CourseResources.FirstOrDefaultAsync(r => r.Type == ResourceType.Assessment && r.Id == resourceId, ct);
        if (resource is null)
            return Result<bool>.Failure(AssessmentErrors.NotFound);

        resource.IsPublished = isPublished;
        await db.SaveChangesAsync(ct);

        return Result<bool>.Success(resource.IsPublished);
    }
}
