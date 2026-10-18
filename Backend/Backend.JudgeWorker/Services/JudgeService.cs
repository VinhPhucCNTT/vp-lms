using Backend.JudgeWorker.Configuration;
using Backend.JudgeWorker.Types;
using Backend.Persistence.Common;
using Backend.Persistence.Entities.Judge;
using Microsoft.Extensions.Options;

namespace Backend.JudgeWorker.Services;

public interface IJudgeService
{
    Task<JudgeResult> JudgeAsync(
        ProblemSubmission submission,
        CancellationToken cancellationToken);
}

public sealed class JudgeService(
    IDockerRunner dockerRunner,
    // IDbContextFactory<AppDbContext> dbFactory,
    IOptions<JudgeOptions> options,
    ILogger<JudgeService> logger)
    : IJudgeService
{
    // private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;

    public async Task<JudgeResult> JudgeAsync(
        ProblemSubmission submission,
        CancellationToken ct = default)
    {
        // using var db = await _dbFactory.CreateDbContextAsync(ct);
        LanguageProfileStruct? profile = await GetLanguageConfigurationAsync(submission, ct);
        if (profile is null)
        {
            logger.LogError("Profile does not exist.");
            return new JudgeResult(
                SubmissionStatus.SystemError,
                0, Results: null);
        }

        var workspace =
            Path.Combine(
                options.Value.WorkspaceRelativeDir,
                "lms-judge",
                submission.Id.ToString());
        Directory.CreateDirectory(workspace);

        string languageName = submission.Compiler.Language.ToString();
        string imageName = GetImageName(languageName);
        try
        {
            await PrepareWorkspaceAsync(
                submission,
                profile,
                submission.Compiler.SourceFileName,
                workspace,
                ct);

            if (!string.IsNullOrEmpty(profile.CompilerCmd))
            {
                logger.LogInformation(
                    "Compiling submission {SubmissionId} using {Language}",
                    submission.Id,
                    submission.Compiler.Name);

                var compileResult =
                    await dockerRunner.CompileAsync(
                        profile,
                        imageName,
                        workspace,
                        submission.Problem.MemoryLimitMb,
                        ct);

                if (compileResult.ExitCode != 0)
                {
                    return new JudgeResult(
                        SubmissionStatus.CompilationError,
                        ExecutionTimeMs: compileResult.ExecutionTimeMs,
                        Results: null,
                        CompileError:
                            compileResult.StandardError);
                }
            }

            double totalExecutionTime = 0;
            List<TestCaseResult> testResults = [];

            var testCases = submission.Problem.TestCases.OrderBy(x => x.OrderIndex);
            int timeLimitMs = submission.Problem.TimeLimitMs;
            int memoryLimitMb = submission.Problem.MemoryLimitMb;
            foreach (var testCase in testCases)
            {
                ct.ThrowIfCancellationRequested();

                logger.LogInformation(
                    "Running submission {SubmissionId}, language {Language}, test #{Test}",
                    submission.Id,
                    submission.Compiler.Language.ToString(),
                    testCase.OrderIndex);

                var result =
                    await dockerRunner.ExecuteAsync(
                        profile,
                        imageName,
                        workspace,
                        testCase.Input,
                        timeLimitMs,
                        memoryLimitMb,
                        ct);
                totalExecutionTime +=
                    result.ExecutionTimeMs;

                if (result.TimedOut)
                {
                    // TODO: Implement memory
                    testResults.Append(new TestCaseResult(
                        testCase.OrderIndex,
                        false,
                        result.ExecutionTimeMs, 0));

                    return new JudgeResult(
                        SubmissionStatus.TimeLimitExceeded,
                        result.ExecutionTimeMs,
                        ErrorTestIdx: testCase.OrderIndex,
                        Results: testResults);
                }

                if (result.ExitCode != 0)
                {
                    testResults.Append(new TestCaseResult(
                        testCase.OrderIndex,
                        false,
                        result.ExecutionTimeMs, 0));

                    return new JudgeResult(
                        SubmissionStatus.RuntimeError,
                        result.ExecutionTimeMs,
                        ErrorTestIdx: testCase.OrderIndex,
                        Results: testResults,
                        RuntimeError:
                            result.StandardError);
                }

                if (!OutputsEqual(
                        result.StandardOutput,
                        testCase.ExpectedOutput))
                {
                    testResults.Append(new TestCaseResult(
                        testCase.OrderIndex,
                        false,
                        result.ExecutionTimeMs, 0));

                    return new JudgeResult(
                        SubmissionStatus.WrongAnswer,
                        result.ExecutionTimeMs,
                        ErrorTestIdx: testCase.OrderIndex,
                        Results: testResults,
                        RuntimeOutput:
                            result.StandardOutput);
                }

                testResults.Append(new TestCaseResult(
                    testCase.OrderIndex,
                    true,
                    result.ExecutionTimeMs, 0));
            }

            return new JudgeResult(
                SubmissionStatus.Accepted,
                totalExecutionTime,
                Results: testResults);
        }
        catch (OperationCanceledException)
            when (ct.IsCancellationRequested)
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
                SubmissionStatus.SystemError,
                0,
                Results: null,
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

    // NOTE: Currently the system assumes that each language has a single compiler
    private async Task<LanguageProfileStruct?> GetLanguageConfigurationAsync(
        ProblemSubmission submission,
        CancellationToken ct = default)
    {
        var compiler = submission.Compiler;
        System.Diagnostics.Debug.Assert(compiler is not null);

        LanguageProfileStruct? profile = submission.Problem.LanguageProfile.FirstOrDefault(p => p.CompilerId == compiler.Id);
        if (profile is null)
        {
            logger.LogError($"Langauge profile for {compiler.Language} is not defined for problem #{submission.ProblemId}.");
            return null;
        }

        return profile;
    }

    private static async Task PrepareWorkspaceAsync(
        ProblemSubmission submission,
        LanguageProfileStruct profile,
        string sourceFileName,
        string workspace,
        CancellationToken ct = default)
    {
        var sourcePath =
            Path.Combine(
                workspace,
                sourceFileName);

        await File.WriteAllTextAsync(
            sourcePath,
            submission.SubmittedCode,
            ct);

        if (submission.Compiler.Language ==
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
                ct);
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

    private static string GetImageName(string languageName)
    {
        return String.Concat("judge-", languageName.ToLowerInvariant());
    }
}
