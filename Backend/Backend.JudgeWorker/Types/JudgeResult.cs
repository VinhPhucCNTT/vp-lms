using Backend.Persistence.Entities.Judge;

namespace Backend.JudgeWorker.Types;

public sealed record TestCaseResult(
    int Idx,
    bool IsAccepted,
    double ExecutionTimeMs,
    int MemoryUsedKb
);

public sealed record JudgeResult(
    SubmissionStatus Status,
    double ExecutionTimeMs,
    int? ErrorTestIdx = null,
    List<TestCaseResult>? Results = null,
    string? CompileError = null,
    string? RuntimeOutput = null,
    string? RuntimeError = null
);
