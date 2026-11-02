using Backend.Judge.Contracts;
using Backend.JudgeWorker.Configuration;
using Backend.JudgeWorker.Types;
using Backend.Persistence.Data;
using Backend.Persistence.Entities.Content;
using Backend.Persistence.Entities.Judge;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Backend.JudgeWorker.Services;

public interface IJudgeService
{
    public Task<JudgeResult> JudgeAsync(
        AppDbContext db,
        ProblemSubmission submission,
        CancellationToken cancellationToken);
}

public sealed class JudgeService(
    IDockerRunner dockerRunner,
    IOptions<JudgeOptions> options,
    ILogger<JudgeService> logger)
    : IJudgeService
{
    private readonly ILogger<JudgeService> logger = logger;

    public async Task<JudgeResult> JudgeAsync(
        AppDbContext db,
        ProblemSubmission submission,
        CancellationToken ct = default)
    {
        CompilerProfile profile = await db.CompilerProfiles.FirstAsync(p => p.Id == submission.ProfileId, ct);
        Compiler compiler = await db.Compilers.FirstAsync(c => c.Id == profile.CompilerId, ct);
        CodingProblem problem = await db.CodingProblems.FirstAsync(p => p.Id == submission.ProblemId, ct);

        var workspaceFullPath = Path.GetFullPath(options.Value.WorkspaceRelativeDir);
        var workspace =
            Path.Combine(
                workspaceFullPath,
                "lms-judge",
                submission.Id.ToString()).Replace('\\', '/');
        Directory.CreateDirectory(workspace);

        string languageName = compiler.Language.ToString();
        string imageName = GetImageName(languageName);
        await PrepareWorkspaceAsync(
            submission,
            profile,
            compiler.SourceFileName,
            workspace,
            ct);

        try
        {
            if (!string.IsNullOrEmpty(profile.CompileCmd))
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation(
                        "Compiling submission {SubmissionId} using {Language}",
                        submission.Id,
                        compiler.Name);
                }

                DockerExecutionResult compileResult =
                    await dockerRunner.CompileAsync(
                        profile,
                        imageName,
                        workspace,
                        problem.MemoryLimitMb,
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
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Compiliation failed for submission {SubmissionId}.", submission.Id);

            return new JudgeResult(
                SubmissionStatus.SystemError,
                0);

        }

        double totalExecutionTime = 0;
        List<TestCaseResult> testResults = [];

        try
        {
            var testCases = await db.ProblemTestCases
                .Where(t => t.ProblemId == problem.Id)
                .OrderBy(x => x.OrderIndex)
                .ToListAsync(ct);

            if (testCases.Count == 0)
            {
                logger.LogWarning("No test case found.");
                throw new Exception("No test case found.");
            }

            int timeLimitMs = problem.TimeLimitMs;
            int memoryLimitMb = problem.MemoryLimitMb;
            foreach (var testCase in testCases)
            {
                ct.ThrowIfCancellationRequested();

                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation(
                        "Running submission {SubmissionId}, language {Language}, test #{Test}",
                        submission.Id,
                        compiler.Language.ToString(),
                        testCase.OrderIndex);
                }

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
                    testResults.Add(new TestCaseResult(
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
                    testResults.Add(new TestCaseResult(
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
                    testResults.Add(new TestCaseResult(
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

                testResults.Add(new TestCaseResult(
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
    // private async Task<LanguageProfileStruct?> GetLanguageConfigurationAsync(
    //     ProblemSubmission submission,
    //     CancellationToken ct = default)
    // {
    //     var compiler = submission.Compiler;
    //     System.Diagnostics.Debug.Assert(compiler is not null);
    //
    //     LanguageProfileStruct? profile = submission.Problem.LanguageProfile.FirstOrDefault(p => p.CompilerId == compiler.Id);
    //     if (profile is null)
    //     {
    //         logger.LogError($"Langauge profile for {compiler.Language} is not defined for problem #{submission.ProblemId}.");
    //         return null;
    //     }
    //
    //     return profile;
    // }

    private static async Task PrepareWorkspaceAsync(
        ProblemSubmission submission,
        CompilerProfile profile,
        string sourceFileName,
        string workspace,
        CancellationToken ct = default)
    {
        var sourcePath =
            Path.Combine(
                workspace,
                sourceFileName);

        // var normalizedCode = submission.SubmittedCode
        //     .Replace("\r\n", "\n")
        //     .Replace("\r", "\n")
        //     .Replace("\n", Environment.NewLine);

        await File.WriteAllTextAsync(
            sourcePath,
            submission.SubmittedCode,
            ct);

        if (profile.Compiler.Language ==
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
