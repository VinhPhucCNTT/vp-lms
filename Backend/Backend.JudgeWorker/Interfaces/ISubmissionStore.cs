using Backend.Judge.Contracts;

namespace Backend.JudgeWorker.Interfaces;

public interface ISubmissionStore
{
    Task<JudgeSubmission?> GetForJudgingAsync(
        long submissionId,
        CancellationToken cancellationToken);

    Task MarkRunningAsync(
        long submissionId,
        CancellationToken cancellationToken);

    Task CompleteAsync(
        long submissionId,
        JudgeResult result,
        CancellationToken cancellationToken);

    Task MarkSystemErrorAsync(
        long submissionId,
        string error,
        CancellationToken cancellationToken);
}
