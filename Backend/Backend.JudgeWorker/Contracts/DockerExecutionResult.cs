namespace Backend.JudgeWorker.Contracts;

public sealed record DockerExecutionResult(
    int ExitCode,
    string StandardOutput,
    string StandardError,
    long ExecutionTimeMs,
    bool TimedOut
);
