using Backend.JudgeWorker.Contracts;
using Backend.JudgeWorker.Interfaces;

namespace Backend.JudgeWorker.Services;

public interface ISubmissionProcessor
{
    Task ProcessAsync(
        long submissionId,
        CancellationToken cancellationToken);
}

public sealed class SubmissionProcessor(
    ISubmissionStore submissionStore,
    IJudgeService judgeService,
    ILogger<SubmissionProcessor> logger)
    : ISubmissionProcessor
{
    public async Task ProcessAsync(
        long submissionId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing submission {SubmissionId}",
            submissionId);

        var submission =
            await submissionStore.GetForJudgingAsync(
                submissionId,
                cancellationToken);

        if (submission is null)
        {
            logger.LogWarning(
                "Submission {SubmissionId} no longer exists",
                submissionId);

            return;
        }

        await submissionStore.MarkRunningAsync(
            submissionId,
            cancellationToken);

        try
        {
            var result =
                await judgeService.JudgeAsync(
                    submission,
                    cancellationToken);

            await submissionStore.CompleteAsync(
                submissionId,
                result,
                cancellationToken);

            logger.LogInformation(
                "Submission {SubmissionId} completed with {Verdict}",
                submissionId,
                result.Verdict);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to process submission {SubmissionId}",
                submissionId);

            await submissionStore.MarkSystemErrorAsync(
                submissionId,
                ex.Message,
                cancellationToken);

            throw;
        }
    }
}
