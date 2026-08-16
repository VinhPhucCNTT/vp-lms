namespace Backend.Api.Core.Types;

public record CodingProblemInfo(
    string ProblemStatementMarkdown,
    string? ConstraintsMarkdown,
    string FunctionSignature,
    string Language,
    int TimeLimitMs,
    int MemoryLimitMb,
    bool IsPractice
);

public record CodingProblemRequest(
    ResourceRequestInfo ResourceInfo,
    CodingProblemInfo Info
);
