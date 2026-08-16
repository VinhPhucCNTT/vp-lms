using System.Text;
using Backend.JudgeWorker.Contracts;
using Backend.JudgeWorker.Interfaces;

namespace Backend.JudgeWorker.Services;

public sealed class JudgeService(
    IDockerRunner dockerRunner,
    ILogger<JudgeService> logger) : IJudgeService
{
    public async Task<JudgeResult> JudgeAsync(
        SubmissionToJudge submission,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(
                submission.Language,
                "cpp",
                StringComparison.OrdinalIgnoreCase))
        {
            return new JudgeResult(
                JudgeVerdict.SystemError,
                0,
                RuntimeError:
                    $"Unsupported language: {submission.Language}");
        }

        var workspace =
            Path.Combine(
                Path.GetTempPath(),
                "lms-judge",
                submission.Id.ToString());

        Directory.CreateDirectory(workspace);

        try
        {
            var sourcePath =
                Path.Combine(
                    workspace,
                    "Main.cpp");

            await File.WriteAllTextAsync(
                sourcePath,
                submission.SourceCode,
                Encoding.UTF8,
                cancellationToken);

            logger.LogInformation(
                "Compiling submission {SubmissionId}",
                submission.Id);

            var compileResult =
                await dockerRunner.CompileAsync(
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

            logger.LogInformation(
                "Compilation successful for submission {SubmissionId}",
                submission.Id);

            foreach (var testCase in submission.TestCases
                         .OrderBy(x => x.OrderIndex))
            {
                cancellationToken.ThrowIfCancellationRequested();

                logger.LogInformation(
                    "Running submission {SubmissionId}, test {Test}",
                    submission.Id,
                    testCase.OrderIndex);

                var executionResult =
                    await dockerRunner.ExecuteAsync(
                        workspace,
                        testCase.Input,
                        submission.TimeLimitMs,
                        submission.MemoryLimitMb,
                        cancellationToken);

                if (executionResult.TimedOut)
                {
                    return new JudgeResult(
                        JudgeVerdict.TimeLimitExceeded,
                        executionResult.ExecutionTimeMs);
                }

                if (executionResult.ExitCode != 0)
                {
                    return new JudgeResult(
                        JudgeVerdict.RuntimeError,
                        executionResult.ExecutionTimeMs,
                        RuntimeError:
                            executionResult.StandardError);
                }

                if (!OutputsEqual(
                        executionResult.StandardOutput,
                        testCase.ExpectedOutput))
                {
                    return new JudgeResult(
                        JudgeVerdict.WrongAnswer,
                        executionResult.ExecutionTimeMs,
                        RuntimeOutput:
                            executionResult.StandardOutput);
                }
            }

            return new JudgeResult(
                JudgeVerdict.Accepted,
                0);
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
                "Unexpected error while judging submission {SubmissionId}",
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
                    "Failed to clean workspace {Workspace}",
                    workspace);
            }
        }
    }

    private static bool OutputsEqual(
        string actual,
        string expected)
    {
        static string Normalize(string value)
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
