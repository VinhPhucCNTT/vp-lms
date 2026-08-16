using Backend.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Backend.Api.Services.Common;
using Sqids;
using AutoMapper;
using Backend.Persistence.Entities.Assessments;
using Backend.Api.Core.Common;
using Backend.Api.Core.Types;
using System.Text.Json;

namespace Backend.Api.Services.Assessments;

public sealed class AssessmentAttemptService(
    IDbContextFactory<AppDbContext> dbFactory,
    CurrentUserService currentUserService,
    IMapper mapper,
    SqidsEncoder<long> sqidsEncoder,
    QuestionSelectionService questionSelection,
    AssessmentGradingService grading)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly CurrentUserService _currentUserService = currentUserService;
    private readonly IMapper _mapper = mapper;
    private readonly SqidsEncoder<long> _sqidsEncoder = sqidsEncoder;

    public async Task<Result<AssessmentAttempt>> StartAsync(
           long assessmentId,
           CancellationToken ct = default)
    {
        var studentId = _currentUserService.UserId;
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var assessment = await db.Assessments
            .AsNoTracking()
            .Include(x => x.Resource)
            .FirstOrDefaultAsync(x => x.Id == assessmentId, ct);

        if (assessment is null)
            return Result<AssessmentAttempt>.Failure(
                AssessmentErrors.NotFound);

        if (!assessment.Resource.IsPublished)
        {
            return Result<AssessmentAttempt>.Failure(
                new Error(
                    "assessment.unavailable",
                    "The assessment is not available."));
        }

        var now = DateTime.UtcNow;

        if (assessment.AvailableFrom.HasValue &&
            now < assessment.AvailableFrom)
        {
            return Result<AssessmentAttempt>.Failure(
                new Error(
                    "assessment.not_started",
                    "The assessment is not yet available."));
        }

        if (assessment.AvailableUntil.HasValue &&
                    now >= assessment.AvailableUntil)
        {
            return Result<AssessmentAttempt>.Failure(
                new Error(
                    "assessment.closed",
                    "The assessment is no longer available."));
        }

        // var enrolled = await db.Enrollments
        //     .AnyAsync(
        //         x => x.CourseId == assessment. &&
        //              x.StudentId == studentId,
        //         ct);
        // if (!enrolled)
        //     return Result<AssessmentAttempt>.Failure(
        //         new Error(
        //             "assessment.not_enrolled",
        //             "The student is not enrolled in this course."));

        var attemptCount = await db.AssessmentAttempts
            .CountAsync(
                x => x.AssessmentId == assessmentId &&
                     x.StudentId == studentId,
                ct);

        if (attemptCount >= assessment.MaxAttempts)
        {
            return Result<AssessmentAttempt>.Failure(
                new Error(
                    "assessment.max_attempts",
                    "The maximum number of attempts has been reached."));
        }

        var existingAttempt = await db.AssessmentAttempts
            .FirstOrDefaultAsync(
                x => x.AssessmentId == assessmentId &&
                     x.StudentId == studentId &&
                     x.Status == AssessmentAttemptStatus.InProgress,
                ct);

        if (existingAttempt is not null)
        {
            return Result<AssessmentAttempt>.Success(
                existingAttempt);
        }

        var attempt = new AssessmentAttempt
        {
            AssessmentId = assessmentId,
            StudentId = studentId,
            AttemptNumber = attemptCount + 1,
            StartedAt = now,
            Status = AssessmentAttemptStatus.InProgress
        };

        db.AssessmentAttempts.Add(attempt);

        await db.SaveChangesAsync(ct);

        var selected = await questionSelection
            .SelectForAttemptAsync(
                assessmentId,
                ct);

        if (!selected.IsSuccess)
            return Result<AssessmentAttempt>.Failure(
                selected.Errors.ToArray());

        foreach (var selectedQuestion in selected.Value!)
        {
            attempt.Questions.Add(
                new AttemptQuestion
                {
                    AssessmentQuestionId = selectedQuestion.QuestionId,
                    OrderIndex = selectedQuestion.OrderIndex,
                    Points = selectedQuestion.Points
                });
        }

        await db.SaveChangesAsync(ct);

        return Result<AssessmentAttempt>.Success(attempt);
    }

    public async Task<Result<AssessmentAttempt>> GetAsync(
        long attemptId,
        CancellationToken ct = default)
    {
        var studentId = _currentUserService.UserId;
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var attempt = await db.AssessmentAttempts
            .AsNoTracking()
            .Include(x => x.Questions)
                .ThenInclude(x => x.Answer)
            .FirstOrDefaultAsync(
                x => x.Id == attemptId &&
                     x.StudentId == studentId,
                ct);

        return attempt is null
            ? Result<AssessmentAttempt>.Failure(
                new Error(
                    "attempt.not_found",
                    "Assessment attempt was not found."))
            : Result<AssessmentAttempt>.Success(attempt);
    }

    public async Task<Result> SaveAnswerAsync(
        long attemptId,
        long attemptQuestionId,
        JsonDocument answer,
        CancellationToken ct = default)
    {
        var studentId = _currentUserService.UserId;
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var attemptQuestion = await db.AttemptQuestions
            .Include(x => x.Attempt)
            .Include(x => x.Answer)
            .FirstOrDefaultAsync(
                x => x.Id == attemptQuestionId &&
                     x.AttemptId == attemptId &&
                     x.Attempt.StudentId == studentId,
                ct);

        if (attemptQuestion is null)
        {
            return Result.Failure(
                new Error(
                    "attempt_question.not_found",
                    "Attempt question was not found."));
        }

        if (!IsAttemptActive(
                attemptQuestion.Attempt,
                out var expirationError))
        {
            return Result.Failure(expirationError!);
        }

        if (attemptQuestion.Answer is null)
        {
            attemptQuestion.Answer = new AttemptAnswer
            {
                AnswerData = CloneJson(answer),
                AnsweredAt = DateTime.UtcNow
            };
        }
        else
        {
            attemptQuestion.Answer.AnswerData = CloneJson(answer);
            attemptQuestion.Answer.AnsweredAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> SetFlagAsync(
        long attemptId,
        long attemptQuestionId,
        bool flagged,
        CancellationToken ct = default)
    {
        var studentId = _currentUserService.UserId;
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var question = await db.AttemptQuestions
            .Include(x => x.Attempt)
            .FirstOrDefaultAsync(
                x => x.Id == attemptQuestionId &&
                     x.AttemptId == attemptId &&
                     x.Attempt.StudentId == studentId,
                ct);

        if (question is null)
        {
            return Result.Failure(
                new Error(
                    "attempt_question.not_found",
                    "Attempt question was not found."));
        }

        if (!IsAttemptActive(
            question.Attempt,
            out var expirationError))
        {
            return Result.Failure(expirationError!);
        }

        question.IsFlagged = flagged;
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> SubmitAsync(
        long attemptId,
        CancellationToken ct = default)
    {
        var studentId = _currentUserService.UserId;
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var attempt = await db.AssessmentAttempts
            .FirstOrDefaultAsync(
                x => x.Id == attemptId &&
                     x.StudentId == studentId,
                ct);

        if (attempt is null)
        {
            return Result.Failure(
                new Error(
                    "attempt.not_found",
                    "Assessment attempt was not found."));
        }

        if (attempt.Status != AssessmentAttemptStatus.InProgress)
        {
            return Result.Failure(
                new Error(
                    "attempt.not_active",
                    "The assessment attempt is no longer active."));
        }

        var deadline = await db.Assessments
            .Where(x => x.Id == attempt.AssessmentId)
            .Select(x => x.TimeLimitMinutes)
            .FirstAsync(ct);

        var expired =
            DateTime.UtcNow >=
            attempt.StartedAt.AddMinutes(deadline);

        attempt.SubmittedAt = DateTime.UtcNow;
        attempt.Status = expired
            ? AssessmentAttemptStatus.Expired
            : AssessmentAttemptStatus.Submitted;

        await db.SaveChangesAsync(ct);

        var gradeResult = await grading.GradeAsync(
            attemptId,
            ct);

        if (!gradeResult.IsSuccess)
            return Result.Failure(gradeResult.Errors.ToArray());

        return Result.Success();
    }


    private static bool IsAttemptActive(
        AssessmentAttempt attempt,
        out Error? error)
    {
        if (attempt.Status != AssessmentAttemptStatus.InProgress)
        {
            error = new Error(
                "attempt.not_active",
                "The assessment attempt is no longer active.");

            return false;
        }

        error = null;
        return true;
    }

    private static JsonDocument CloneJson(JsonDocument json) =>
        JsonDocument.Parse(json.RootElement.GetRawText());

    // private static void EnsureAttemptActive(
    //     AssessmentAttempt attempt)
    // {
    //     if (attempt.Status != AssessmentAttemptStatus.InProgress)
    //         throw new ValidationException(
    //             "The attempt is no longer active.");
    //
    //     var deadline =
    //         attempt.StartedAt.AddMinutes(
    //             attempt.Assessment.TimeLimitMinutes);
    //
    //     if (DateTime.UtcNow >= deadline)
    //     {
    //         throw new AttemptExpiredException();
    //     }
    // }
    //
    // private static void ValidateCanStart(
    //     Assessment assessment)
    // {
    //     if (!assessment.IsPublished)
    //         throw new ValidationException(
    //             "Assessment is not published.");
    //
    //     var now = DateTime.UtcNow;
    //
    //     if (assessment.AvailableFrom.HasValue &&
    //         now < assessment.AvailableFrom.Value)
    //     {
    //         throw new ValidationException(
    //             "Assessment is not yet available.");
    //     }
    //
    //     if (assessment.AvailableUntil.HasValue &&
    //         now > assessment.AvailableUntil.Value)
    //     {
    //         throw new ValidationException(
    //             "Assessment is no longer available.");
    //     }
    // }
}
