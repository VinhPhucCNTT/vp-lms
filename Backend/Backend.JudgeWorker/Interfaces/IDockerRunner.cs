using Backend.JudgeWorker.Contracts;

namespace Backend.JudgeWorker.Interfaces;

public interface IDockerRunner
{
    Task<DockerExecutionResult> CompileAsync(
        string workspace,
        CancellationToken cancellationToken);

    Task<DockerExecutionResult> ExecuteAsync(
        string workspace,
        string input,
        int timeLimitMs,
        int memoryLimitMb,
        CancellationToken cancellationToken);
}
