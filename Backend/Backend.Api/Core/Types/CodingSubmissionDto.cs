using Backend.Persistence.Entities.Judge;

namespace Backend.Api.Core.Types;

public sealed record CreateCodingSubmissionRequest(
    string SourceCode,
    ProgrammingLanguage Language
);

public sealed record CodingSubmissionResponse(
    string Id,
    string Status,
    double? ExecutionTimeMs,
    int? MemoryUsedKb,
    int? PassedTestCases,
    int? TotalTestCases
);
