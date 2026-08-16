using Backend.JudgeWorker.Contracts;
using Backend.JudgeWorker.Languages;

namespace Backend.JudgeWorker.Interfaces;

public interface IDockerRunner
{
    Task<DockerExecutionResult> CompileAsync(
        LanguageDefinition language,
        string workspace,
        CancellationToken cancellationToken);

    Task<DockerExecutionResult> ExecuteAsync(
        LanguageDefinition language,
        string workspace,
        string input,
        int timeLimitMs,
        int memoryLimitMb,
        CancellationToken cancellationToken);
}
