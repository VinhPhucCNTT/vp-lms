using Backend.Persistence.Entities.Judge;

namespace Backend.Api.Core.Types;

public sealed record ProblemSubmissionRequest(
    ProgrammingLanguage Language,
    string SourceCode
);

public sealed record ProblemSubmissionResponse(
    string Id,
    int OrderIndex,
    string SubmittedCode,
    SubmissionStatus Status,
    string? CompileError
) : IEntityResponse;

public sealed record CompilerRequest(
    string Name,
    ProgrammingLanguage Language,
    string SourceFileName,
    string ApplicationName,
    string Version
);

public sealed record CompilerQueryRequest(
    int PageNumber,
    int PageSize,
    string? Name,
    ProgrammingLanguage? Language,
    string? SourceFileName,
    string? ApplicationName,
    string? Version
);

public sealed record CompilerResponse(
    string Id,
    string Name,
    ProgrammingLanguage Language,
    string SourceFileName,
    string ApplicationName,
    string Version
) : IEntityResponse;

public sealed record CompilerProfileResponse(
    CompilerResponse CompilerInfo,
    string Name,
    string CompileCmd,
    string RunCmd
);

public sealed record CompilerProfileRequest(
    string Name,
    string CompileCmd,
    string RunCmd
);
