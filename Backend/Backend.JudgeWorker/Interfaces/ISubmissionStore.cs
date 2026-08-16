using Backend.JudgeWorker.Contracts;

namespace Backend.JudgeWorker.Interfaces;

public interface ISubmissionStore
{
    Task<SubmissionToJudge?> GetForJudgingAsync(
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
