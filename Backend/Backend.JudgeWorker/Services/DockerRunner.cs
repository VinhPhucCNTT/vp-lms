using System.Diagnostics;
using Backend.JudgeWorker.Contracts;
using Backend.JudgeWorker.Interfaces;
using Backend.JudgeWorker.Languages;

namespace Backend.JudgeWorker.Services;

public sealed class DockerRunner(
    ILogger<DockerRunner> logger)
    : IDockerRunner
{
    public async Task<DockerExecutionResult> CompileAsync(
        LanguageDefinition language,
        string workspace,
        CancellationToken cancellationToken)
    {
        if (!language.RequiresCompilation)
        {
            return new DockerExecutionResult(
                ExitCode: 0,
                StandardOutput: "",
                StandardError: "",
                ExecutionTimeMs: 0,
                TimedOut: false);
        }

        var arguments = BuildDockerArguments(
            language,
            workspace,
            language.CompileCommand);

        return await RunDockerAsync(
            arguments,
            timeoutMs: 30_000,
            input: null,
            cancellationToken);
    }

    public async Task<DockerExecutionResult> ExecuteAsync(
        LanguageDefinition language,
        string workspace,
        string input,
        int timeLimitMs,
        int memoryLimitMb,
        CancellationToken cancellationToken)
    {
        var arguments = BuildDockerArguments(
            language,
            workspace,
            language.RunCommand,
            memoryLimitMb);

        try
        {
            return await RunDockerAsync(
                arguments,
                timeoutMs: timeLimitMs,
                input,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
    }

    private static List<string> BuildDockerArguments(
        LanguageDefinition language,
        string workspace,
        string command,
        int? memoryLimitMb = null)
    {
        var containerName =
            $"judge-{Guid.NewGuid():N}";

        var arguments = new List<string>
        {
            "run",
            "--rm",

            "--name",
            containerName,

            "--network",
            "none",

            "--cpus",
            "1",

            "--pids-limit",
            "64",

            "--cap-drop",
            "ALL",

            "--security-opt",
            "no-new-privileges",

            "--mount",
            $"type=bind,source={workspace},target=/workspace",

            "--workdir",
            "/workspace"
        };

        if (memoryLimitMb.HasValue)
        {
            arguments.Add("--memory");
            arguments.Add($"{memoryLimitMb.Value}m");
        }

        arguments.Add(language.ImageName);

        arguments.Add("sh");
        arguments.Add("-c");
        arguments.Add(command);

        return arguments;
    }

    private async Task<DockerExecutionResult> RunDockerAsync(
        List<string> arguments,
        int timeoutMs,
        string? input,
        CancellationToken cancellationToken)
    {
        var stopwatch =
            Stopwatch.StartNew();

        var startInfo =
            new ProcessStartInfo
            {
                FileName = "docker",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process =
            new Process
            {
                StartInfo = startInfo
            };

        process.Start();

        if (input is not null)
        {
            await process.StandardInput.WriteAsync(input);
        }

        process.StandardInput.Close();

        using var timeoutCts =
            CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);

        timeoutCts.CancelAfter(timeoutMs);

        try
        {
            await process.WaitForExitAsync(
                timeoutCts.Token);
        }
        catch (OperationCanceledException)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(
                        entireProcessTree: true);
                }
            }
            catch
            {
                // Process may have already exited.
            }

            throw;
        }

        var stdout =
            await process.StandardOutput.ReadToEndAsync();

        var stderr =
            await process.StandardError.ReadToEndAsync();

        stopwatch.Stop();

        return new DockerExecutionResult(
            process.ExitCode,
            stdout,
            stderr,
            (long)stopwatch.Elapsed.TotalMilliseconds,
            false);
    }
}
