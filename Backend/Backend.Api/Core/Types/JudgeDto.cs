using Backend.Judge.Contracts;
using Backend.Persistence.Entities.Judge;

namespace Backend.Api.Core.Types;

public sealed record JudgeRequest(
    ProgrammingLanguage Language,
    string SourceCode
);

public sealed record JudgeResponse(
    int OrderIndex,
    string SubmittedCode,
    ProgrammingLanguage Language,
    SubmissionStatus Status,
    int ExecutionTimeMs,
    int MemoryUsedKb
);

public sealed record CompilerDto(
    string Name,
    string ApplicationName,
    string Version
);

public sealed record LanguageProfileDto(
    CompilerDto ComplierInfo,
    string Name,
    string[] CompilerCmd,
    string[] RunCmd
);
