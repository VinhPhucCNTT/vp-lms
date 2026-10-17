using Backend.JudgeWorker.Types;
using Backend.Persistence.Data;
using Backend.Persistence.Entities.Judge;
using Microsoft.EntityFrameworkCore;

namespace Backend.JudgeWorker.Services;

public interface ISubmissionProcessor
{
    Task ProcessAsync(
        long submissionId,
        CancellationToken cancellationToken);
}

public sealed class SubmissionProcessor(
    IDbContextFactory<AppDbContext> dbFactory,
    IJudgeService judgeService,
    ILogger<SubmissionProcessor> logger)
    : ISubmissionProcessor
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;

    public async Task ProcessAsync(
        long submissionId,
        CancellationToken ct = default)
    {
        logger.LogInformation(
            "Processing submission {SubmissionId}",
            submissionId);

        using var db = await _dbFactory.CreateDbContextAsync(ct);

        var submission = await db.ProblemSubmissions.SingleAsync(s => s.Id == submissionId);
        if (submission is null)
        {
            logger.LogWarning(
                "Submission {SubmissionId} no longer exists",
                submissionId);

            return;
        }

        submission.Status = Persistence.Entities.Judge.SubmissionStatus.Running;
        await db.SaveChangesAsync(ct);

        try
        {
            var result =
                await judgeService.JudgeAsync(
                    submission,
                    ct);

            await SaveTestCasesAsync(db, submission, result, ct);

            submission.Status = result.Status;
            submission.CompileError = result.CompileError;
            await db.SaveChangesAsync(ct);

            logger.LogInformation(
                "Submission {SubmissionId} completed with {Verdict}",
                submissionId,
                result.Status);
        }
        catch (OperationCanceledException)
            when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to process submission {SubmissionId}",
                submissionId);

            submission.Status = SubmissionStatus.RuntimeError;

            throw;
        }
    }

    private async Task
        SaveTestCasesAsync(
            AppDbContext db,
            ProblemSubmission submission,
            JudgeResult judgeResult,
            CancellationToken ct = default)
    {
        if (judgeResult.Results is null)
        {
            logger.LogInformation("SaveTestCases(): No test cases in result.");
            return;
        }

        var testCases = submission.Problem.TestCases.OrderBy(t => t.OrderIndex);
        if (testCases is null)
        {
            logger.LogWarning("SaveTestCases(): No test cases defined for submission id {SubmissionId}.", submission.Id);
            return;
        }

        int count = 0;
        foreach (var testCase in testCases)
        {
            var testCaseResult = judgeResult.Results.SingleOrDefault(t => t.Idx == testCase.OrderIndex);
            if (testCaseResult is null) {
                logger.LogInformation("SaveTestCases(): Processed {N} test cases.", count);
                if (count != testCase.OrderIndex) // NOTE: Exceptional case.
                    logger.LogWarning("SaveTestCases(): Current test case index does not match counter.");
                continue;
            }

            var testResult = new ProblemTestResult
            {
                SubmissionId = submission.Id,
                TestCaseId = testCase.Id,

                IsAccepted = testCaseResult.IsAccepted,
                ExecutionTimeMs = testCaseResult.ExecutionTimeMs,
                MemoryUsedKb = testCaseResult.MemoryUsedKb,
            };
            if (!testCaseResult.IsAccepted)
            {
                testResult.RuntimeOutput = judgeResult.RuntimeOutput;
                testResult.RuntimeError = judgeResult.RuntimeError;
            }

            db.ProblemTestResults.Add(testResult);
            count++;
        }

        await db.SaveChangesAsync(ct);
    }
}
