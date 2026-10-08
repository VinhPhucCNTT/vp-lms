namespace Backend.Judge.Contracts;

public enum JudgeVerdict
{
    Accepted,
    WrongAnswer,
    CompilationError,
    RuntimeError,
    TimeLimitExceeded,
    MemoryLimitExceeded,
    SystemError
}

public sealed record JudgeResult(
    JudgeVerdict Verdict,
    long ExecutionTimeMs,
    string? CompilerOutput = null,
    string? RuntimeOutput = null,
    string? RuntimeError = null
);
