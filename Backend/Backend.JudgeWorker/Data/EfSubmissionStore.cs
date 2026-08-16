using Backend.JudgeWorker.Contracts;
using Backend.JudgeWorker.Interfaces;
using Backend.Persistence.Data;
using Backend.Persistence.Entities.Judge;
using Microsoft.EntityFrameworkCore;

namespace Backend.JudgeWorker.Data;

public sealed class EfSubmissionStore(
    IDbContextFactory<AppDbContext> dbContextFactory)
    : ISubmissionStore
{
    public async Task<SubmissionToJudge?> GetForJudgingAsync(
        long submissionId,
        CancellationToken cancellationToken)
    {
        await using var db =
            await dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var submission =
            await db.ProblemSubmissions
                .Where(x => x.Id == submissionId)
                .Select(x => new
                {
                    x.Id,
                    x.SubmittedCode,
                    x.Language,
                    x.ProblemId,

                    x.Problem.TimeLimitMs,
                    x.Problem.MemoryLimitMb,

                    TestCases =
                        x.Problem.TestCases
                            .OrderBy(t => t.OrderIndex)
                            .Select(t => new JudgeTestCase(
                                t.OrderIndex,
                                t.Input,
                                t.ExpectedOutput))
                            .ToList()
                })
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (submission is null)
            return null;

        return new SubmissionToJudge(
            submission.Id,
            submission.SubmittedCode,
            (ProgrammingLanguage)(int)submission.Language,
            submission.TimeLimitMs,
            submission.MemoryLimitMb,
            submission.TestCases);
    }

    public async Task MarkRunningAsync(
        long submissionId,
        CancellationToken cancellationToken)
    {
        await using var db =
            await dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var submission =
            await db.ProblemSubmissions
                .SingleAsync(
                    x => x.Id == submissionId,
                    cancellationToken);

        // The persistence model has no separate Running state. Keep the
        // pending state until the judge produces a terminal result.
        await Task.CompletedTask;
    }

    public async Task CompleteAsync(
        long submissionId,
        JudgeResult result,
        CancellationToken cancellationToken)
    {
        await using var db =
            await dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var submission =
            await db.ProblemSubmissions
                .SingleAsync(
                    x => x.Id == submissionId,
                    cancellationToken);

        submission.Status = MapStatus(result.Verdict);

        submission.ExecutionTimeMs =
            (int)Math.Min(result.ExecutionTimeMs, int.MaxValue);

        await db.SaveChangesAsync(
            cancellationToken);
    }

    public async Task MarkSystemErrorAsync(
        long submissionId,
        string error,
        CancellationToken cancellationToken)
    {
        await using var db =
            await dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var submission =
            await db.ProblemSubmissions
                .SingleAsync(
                    x => x.Id == submissionId,
                    cancellationToken);

        submission.Status = SubmissionStatus.RuntimeError;

        await db.SaveChangesAsync(
            cancellationToken);
    }

    private static SubmissionStatus MapStatus(JudgeVerdict verdict) =>
        verdict switch
        {
            JudgeVerdict.Accepted => SubmissionStatus.Accepted,
            JudgeVerdict.WrongAnswer => SubmissionStatus.WrongAnswer,
            JudgeVerdict.CompilationError => SubmissionStatus.CompilationError,
            JudgeVerdict.RuntimeError => SubmissionStatus.RuntimeError,
            JudgeVerdict.TimeLimitExceeded => SubmissionStatus.TimeLimitExceeded,
            JudgeVerdict.MemoryLimitExceeded => SubmissionStatus.MemoryLimitExceeded,
            _ => SubmissionStatus.RuntimeError
        };
}
