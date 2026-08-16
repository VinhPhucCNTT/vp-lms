using Backend.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Backend.Persistence.Entities.Assessments;
using Backend.Api.Core.Common;

namespace Backend.Api.Services.Assessments;

public interface IQuestionGrader
{
    QuestionType Type { get; }

    Result<GradeResult> Grade(
        AssessmentQuestion question,
        AttemptAnswer? answer);
}

public sealed record GradeResult(
    decimal EarnedPoints,
    bool? IsCorrect);

public sealed class AssessmentGradingService(
    IDbContextFactory<AppDbContext> dbFactory,
    IEnumerable<IQuestionGrader> graders)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;

    public async Task<Result> GradeAsync(
            long attemptId,
            CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var attempt = await db.AssessmentAttempts
            .Include(x => x.Questions)
                .ThenInclude(x => x.Answer!)
                .ThenInclude(x => x.AttemptQuestion)
            .FirstOrDefaultAsync(
                x => x.Id == attemptId,
                ct);

        if (attempt is null)
        {
            return Result.Failure(
                new Error(
                    "attempt.not_found",
                    "Assessment attempt was not found."));
        }
        decimal total = 0;

        foreach (var attemptQuestion in attempt.Questions)
        {
            var grader = graders.FirstOrDefault(
                x => x.Type == attemptQuestion.AssessmentQuestion.QuestionType);

            if (grader is null)
            {
                return Result.Failure(
                    new Error(
                        "grading.unsupported_type",
                        $"No grader exists for {attemptQuestion.AssessmentQuestion.QuestionType}."));
            }

            var result = grader.Grade(
                attemptQuestion.AssessmentQuestion,
                attemptQuestion.Answer);

            if (!result.IsSuccess)
                return Result.Failure([.. result.Errors]);

            var grade = result.Value!;

            if (attemptQuestion.Answer is not null)
            {
                attemptQuestion.Answer.EarnedPoints =
                    grade.EarnedPoints;

                attemptQuestion.Answer.IsCorrect =
                    grade.IsCorrect;
            }

            total += grade.EarnedPoints;
        }

        attempt.TotalScore = total;

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
