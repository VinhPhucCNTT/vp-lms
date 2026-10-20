using System.Diagnostics;
using Backend.Judge.Contracts;
using Backend.Persistence.Common;
using Backend.Persistence.Entities.Judge;

namespace Backend.JudgeWorker.Services;

public interface IDockerRunner
{
    Task<DockerExecutionResult> CompileAsync(
        CompilerProfile profile,
        string imageName,
        string workspace,
        int? memoryLimitMb,
        CancellationToken ct);

    Task<DockerExecutionResult> ExecuteAsync(
        CompilerProfile profile,
        string imageName,
        string workspace,
        string input,
        int timeLimitMs,
        int memoryLimitMb,
        CancellationToken ct);
}

public sealed class DockerRunner(
    ILogger<DockerRunner> logger)
    : IDockerRunner
{
    public async Task<DockerExecutionResult> CompileAsync(
        CompilerProfile profile,
        string imageName,
        string workspace,
        int? memoryLimitMb = null,
        CancellationToken ct = default)
    {
        if (String.IsNullOrEmpty(profile.CompileCmd))
        {
            logger.LogError("Profile {ProfileName} has no compile cmd defined.", profile.Name);
            return new DockerExecutionResult(
                ExitCode: 0,
                StandardOutput: "",
                StandardError: "",
                ExecutionTimeMs: 0,
                TimedOut: false);
        }

        var arguments = BuildDockerArguments(
            imageName,
            workspace,
            profile.CompileCmd,
            memoryLimitMb);

        return await RunDockerAsync(
            arguments,
            timeoutMs: 30_000,
            input: null,
            ct);
    }

    public async Task<DockerExecutionResult> ExecuteAsync(
        CompilerProfile profile,
        string imageName,
        string workspace,
        string input,
        int timeLimitMs,
        int memoryLimitMb,
        CancellationToken ct = default)
    {
        var arguments = BuildDockerArguments(
            imageName,
            workspace,
            profile.RunCmd,
            memoryLimitMb);

        try
        {
            return await RunDockerAsync(
                arguments,
                timeoutMs: timeLimitMs,
                input,
                ct);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
    }

    private static List<string> BuildDockerArguments(
        string imageName,
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

        arguments.Add(imageName);

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

            // The judge's time limit expired.
            if (!cancellationToken.IsCancellationRequested)
            {
                stopwatch.Stop();

                return new DockerExecutionResult(
                    ExitCode: -1,
                    StandardOutput: "",
                    StandardError: "",
                    ExecutionTimeMs: stopwatch.Elapsed.TotalMilliseconds,
                    TimedOut: true);
            }

            // The worker itself is shutting down.
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
