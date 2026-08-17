using Backend.Persistence.Entities.Content;

namespace Backend.Api.Core.Types;

public sealed record CodingProblemInfo(
    string ProblemStatementMarkdown,
    string? ConstraintsMarkdown,
    string FunctionSignature,
    string Language,
    int TimeLimitMs,
    int MemoryLimitMb,
    bool IsPractice,
    ProblemDifficulty Difficulty
);

public sealed record CodingProblemRequest(
    ResourceRequestInfo ResourceInfo,
    CodingProblemInfo Info
);

public sealed record CodingProblemResponse(
    ResourceDetailResponse ResourceInfo,
    CodingProblemInfo Info
);

public sealed record ProblemTestCaseRequest(
    string Input,
    string ExpectedOutput
);

public sealed record ProblemTestCaseResponse(
    string Id,
    int OrderIndex,
    string Input,
    string ExpectedOutput
) : IEntityResponse;
