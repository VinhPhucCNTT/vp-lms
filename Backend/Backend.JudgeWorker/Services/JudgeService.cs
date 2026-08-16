using Backend.JudgeWorker.Contracts;
using Backend.JudgeWorker.Interfaces;
using Backend.JudgeWorker.Languages;

namespace Backend.JudgeWorker.Services;

public sealed class JudgeService(
    IDockerRunner dockerRunner,
    ILanguageDefinitionProvider languageProvider,
    ILogger<JudgeService> logger)
    : IJudgeService
{
    public async Task<JudgeResult> JudgeAsync(
        SubmissionToJudge submission,
        CancellationToken cancellationToken)
    {
        var language =
            languageProvider.Get(
                submission.Language);

        var workspace =
            Path.Combine(
                Path.GetTempPath(),
                "lms-judge",
                submission.Id.ToString());

        Directory.CreateDirectory(
            workspace);

        try
        {
            await PrepareWorkspaceAsync(
                submission,
                language,
                workspace,
                cancellationToken);

            if (language.RequiresCompilation)
            {
                logger.LogInformation(
                    "Compiling submission {SubmissionId} using {Language}",
                    submission.Id,
                    submission.Language);

                var compileResult =
                    await dockerRunner.CompileAsync(
                        language,
                        workspace,
                        cancellationToken);

                if (compileResult.ExitCode != 0)
                {
                    return new JudgeResult(
                        JudgeVerdict.CompilationError,
                        compileResult.ExecutionTimeMs,
                        CompilerOutput:
                            compileResult.StandardError);
                }
            }

            long totalExecutionTime = 0;

            foreach (var testCase in submission.TestCases
                         .OrderBy(x => x.OrderIndex))
            {
                cancellationToken.ThrowIfCancellationRequested();

                logger.LogInformation(
                    "Running submission {SubmissionId}, language {Language}, test {Test}",
                    submission.Id,
                    submission.Language,
                    testCase.OrderIndex);

                var result =
                    await dockerRunner.ExecuteAsync(
                        language,
                        workspace,
                        testCase.Input,
                        submission.TimeLimitMs,
                        submission.MemoryLimitMb,
                        cancellationToken);

                totalExecutionTime +=
                    result.ExecutionTimeMs;

                if (result.TimedOut)
                {
                    return new JudgeResult(
                        JudgeVerdict.TimeLimitExceeded,
                        result.ExecutionTimeMs);
                }

                if (result.ExitCode != 0)
                {
                    return new JudgeResult(
                        JudgeVerdict.RuntimeError,
                        result.ExecutionTimeMs,
                        RuntimeError:
                            result.StandardError);
                }

                if (!OutputsEqual(
                        result.StandardOutput,
                        testCase.ExpectedOutput))
                {
                    return new JudgeResult(
                        JudgeVerdict.WrongAnswer,
                        result.ExecutionTimeMs,
                        RuntimeOutput:
                            result.StandardOutput);
                }
            }

            return new JudgeResult(
                JudgeVerdict.Accepted,
                totalExecutionTime);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Judge failed for submission {SubmissionId}",
                submission.Id);

            return new JudgeResult(
                JudgeVerdict.SystemError,
                0,
                RuntimeError: ex.Message);
        }
        finally
        {
            try
            {
                if (Directory.Exists(workspace))
                {
                    Directory.Delete(
                        workspace,
                        recursive: true);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Could not delete workspace {Workspace}",
                    workspace);
            }
        }
    }

    private static async Task PrepareWorkspaceAsync(
        SubmissionToJudge submission,
        LanguageDefinition language,
        string workspace,
        CancellationToken cancellationToken)
    {
        var sourcePath =
            Path.Combine(
                workspace,
                language.SourceFileName);

        await File.WriteAllTextAsync(
            sourcePath,
            submission.SourceCode,
            cancellationToken);

        if (submission.Language ==
            ProgrammingLanguage.CSharp)
        {
            var projectPath =
                Path.Combine(
                    workspace,
                    "Judge.csproj");

            const string project =
                """
                <Project Sdk="Microsoft.NET.Sdk">

                  <PropertyGroup>
                    <OutputType>Exe</OutputType>
                    <TargetFramework>net10.0</TargetFramework>
                    <ImplicitUsings>enable</ImplicitUsings>
                    <Nullable>enable</Nullable>
                  </PropertyGroup>

                </Project>
                """;

            await File.WriteAllTextAsync(
                projectPath,
                project,
                cancellationToken);
        }
    }

    private static bool OutputsEqual(
        string actual,
        string expected)
    {
        static string Normalize(
            string value)
        {
            return value
                .Replace("\r\n", "\n")
                .Trim();
        }

        return string.Equals(
            Normalize(actual),
            Normalize(expected),
            StringComparison.Ordinal);
    }
}
