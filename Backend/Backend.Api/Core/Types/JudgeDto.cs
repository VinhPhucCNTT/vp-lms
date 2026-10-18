using Backend.Persistence.Entities.Judge;

namespace Backend.Api.Core.Types;

public sealed record JudgeTestRequest(
    ProgrammingLanguage Language,
    string SourceCode
);

public sealed record JudgeResponse(
    int OrderIndex,
    string SubmittedCode,
    ProgrammingLanguage Language,
    SubmissionStatus Status,
    double ExecutionTimeMs,
    int MemoryUsedKb
);

public sealed record CompilerDto(
    string Id,
    string Name,
    ProgrammingLanguage Language,
    string SourceFileName,
    string ApplicationName,
    string Version
) : IEntityResponse;

public sealed record CompilerProfileResponse(
    CompilerDto CompilerInfo,
    string Name,
    string CompileCmd,
    string RunCmd
);

public sealed record CompilerProfileRequest(
    string Name,
    string CompileCmd,
    string RunCmd
);
