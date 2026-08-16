namespace Backend.JudgeWorker.Contracts;

public sealed record SubmissionToJudge(
    long Id,
    string SourceCode,
    ProgrammingLanguage Language,
    int TimeLimitMs,
    int MemoryLimitMb,
    IReadOnlyList<JudgeTestCase> TestCases
);
