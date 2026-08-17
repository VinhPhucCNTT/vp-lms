using Backend.Persistence.Common;

namespace Backend.Api.Core.Types;

public sealed record CreateCodingSubmissionRequest(
    string SourceCode,
    ProgrammingLanguage Language
);

public sealed record CodingSubmissionResponse(
    string Id,
    string Status,
    int? ExecutionTimeMs,
    int? MemoryUsedKb,
    int? PassedTestCases,
    int? TotalTestCases
);
