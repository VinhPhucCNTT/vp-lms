using Backend.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Backend.Api.Core.Common;
using Backend.Api.Core.Types;

namespace Backend.Api.Services.Assessments;

public interface IQuestionSelectionService
{
    public Task<Result<IReadOnlyList<SelectedQuestion>>>
        SelectForAttemptAsync(
            long assessmentId,
            CancellationToken ct = default);
}

public sealed class QuestionSelectionService(
    IDbContextFactory<AppDbContext> dbFactory) : IQuestionSelectionService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;

    public async Task<Result<IReadOnlyList<SelectedQuestion>>>
        SelectForAttemptAsync(
            long assessmentId,
            CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var exists = await db.Assessments
            .AnyAsync(x => x.Id == assessmentId, ct);

        if (!exists)
        {
            return Result<IReadOnlyList<SelectedQuestion>>
                .Failure(AssessmentErrors.NotFound);
        }

        var questions = await db.AssessmentQuestions
            .AsNoTracking()
            .Where(x => x.AssessmentId == assessmentId)
            .OrderBy(x => x.OrderIndex)
            .Select(x => new SelectedQuestion(
                x.Id,
                x.OrderIndex,
                x.Points))
            .ToListAsync(ct);

        if (questions.Count == 0)
        {
            return Result<IReadOnlyList<SelectedQuestion>>
                .Failure(
                    new Error(
                        "assessment.no_questions",
                        "The assessment contains no questions."));
        }

        return Result<IReadOnlyList<SelectedQuestion>>
            .Success(questions);
    }
}
