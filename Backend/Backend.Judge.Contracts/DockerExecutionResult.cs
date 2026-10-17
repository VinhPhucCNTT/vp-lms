namespace Backend.Judge.Contracts;

public sealed record DockerExecutionResult(
    int ExitCode,
    string StandardOutput,
    string StandardError,
    double ExecutionTimeMs,
    bool TimedOut
);
