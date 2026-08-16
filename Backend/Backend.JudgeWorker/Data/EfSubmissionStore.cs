using Backend.JudgeWorker.Contracts;
using Backend.JudgeWorker.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.JudgeWorker;

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
            await db.Submissions
                .Where(x => x.Id == submissionId)
                .Select(x => new
                {
                    x.Id,
                    x.SourceCode,
                    x.Language,
                    x.ProblemId,

                    TimeLimitMs = x.Problem.TimeLimitMs,

                    MemoryLimitMb =
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
        {
            return null;
        }

        return new SubmissionToJudge(
            submission.Id,
            submission.SourceCode,
            submission.Language,
            "Main.cpp",
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
            await db.Submissions
                .SingleAsync(
                    x => x.Id == submissionId,
                    cancellationToken);

        submission.Status =
            SubmissionStatus.Running;

        await db.SaveChangesAsync(
            cancellationToken);
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
            await db.Submissions
                .SingleAsync(
                    x => x.Id == submissionId,
                    cancellationToken);

        submission.Status =
            SubmissionStatus.Finished;

        submission.Verdict =
            result.Verdict.ToString();

        submission.ExecutionTimeMs =
            result.ExecutionTimeMs;

        submission.CompilerOutput =
            result.CompilerOutput;

        submission.RuntimeError =
            result.RuntimeError;

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
            await db.Submissions
                .SingleAsync(
                    x => x.Id == submissionId,
                    cancellationToken);

        submission.Status =
            SubmissionStatus.Finished;

        submission.Verdict =
            JudgeVerdict.SystemError.ToString();

        submission.RuntimeError =
            error;

        await db.SaveChangesAsync(
            cancellationToken);
    }
}
