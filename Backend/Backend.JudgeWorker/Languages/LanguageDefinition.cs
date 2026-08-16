using Backend.JudgeWorker.Contracts;

namespace Backend.JudgeWorker.Languages;

public sealed record LanguageDefinition(
    ProgrammingLanguage Language,
    string ImageName,
    string SourceFileName,
    string CompileCommand,
    string RunCommand,
    bool RequiresCompilation
);
