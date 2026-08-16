using Backend.JudgeWorker.Contracts;

namespace Backend.JudgeWorker.Languages;

public interface ILanguageDefinitionProvider
{
    LanguageDefinition Get(ProgrammingLanguage language);
}

public sealed class LanguageDefinitionProvider
    : ILanguageDefinitionProvider
{
    private static readonly Dictionary<
        ProgrammingLanguage,
        LanguageDefinition> Definitions = new()
        {
            [ProgrammingLanguage.Cpp] =
            new(
                ProgrammingLanguage.Cpp,
                "judge-cpp",
                "Main.cpp",
                "g++ Main.cpp -O2 -o Main",
                "./Main",
                true),

            [ProgrammingLanguage.Python] =
            new(
                ProgrammingLanguage.Python,
                "judge-python",
                "Main.py",
                "",
                "python3 Main.py",
                false),

            [ProgrammingLanguage.Java] =
            new(
                ProgrammingLanguage.Java,
                "judge-java",
                "Main.java",
                "javac Main.java",
                "java Main",
                true),

            [ProgrammingLanguage.TypeScript] =
            new(
                ProgrammingLanguage.TypeScript,
                "judge-typescript",
                "Main.ts",
                "tsc Main.ts --target ES2022 --module commonjs --outDir out",
                "node out/Main.js",
                true),

            [ProgrammingLanguage.CSharp] =
            new(
                ProgrammingLanguage.CSharp,
                "judge-csharp",
                "Program.cs",
                "dotnet build Judge.csproj -c Release --ignore-failed-sources",
                "dotnet bin/Release/net10.0/Judge.dll",
                true)
        };

    public LanguageDefinition Get(
        ProgrammingLanguage language)
    {
        if (!Definitions.TryGetValue(
                language,
                out var definition))
        {
            throw new NotSupportedException(
                $"Language '{language}' is not supported.");
        }

        return definition;
    }
}
