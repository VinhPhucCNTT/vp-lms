namespace Backend.JudgeWorker.Contracts;

public sealed record SubmissionToJudge(
    long Id,
    string SourceCode,
    string Language,
    string SourceFileName,
    int TimeLimitMs,
    int MemoryLimitMb,
    IReadOnlyList<JudgeTestCase> TestCases
);
