using Backend.JudgeWorker.Contracts;

namespace Backend.JudgeWorker.Interfaces;

public interface IJudgeService
{
    Task<JudgeResult> JudgeAsync(
        SubmissionToJudge submission,
        CancellationToken cancellationToken);
}
