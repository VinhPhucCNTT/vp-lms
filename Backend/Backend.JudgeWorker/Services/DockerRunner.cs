using System.Diagnostics;
using Backend.JudgeWorker.Contracts;
using Backend.JudgeWorker.Interfaces;

namespace Backend.JudgeWorker.Services;

public sealed class DockerRunner(
    ILogger<DockerRunner> logger) : IDockerRunner
{
    private const string ImageName = "judge-cpp";

    public async Task<DockerExecutionResult> CompileAsync(
        string workspace,
        CancellationToken cancellationToken)
    {
        var arguments = new List<string>
        {
            "run",
            "--rm",

            "--network", "none",

            "--mount",
            $"type=bind,source={workspace},target=/workspace",

            "--workdir", "/workspace",

            ImageName,

            "g++",
            "Main.cpp",
            "-O2",
            "-o",
            "Main"
        };

        return await RunDockerAsync(
            arguments,
            workspace,
            null,
            timeoutMs: 30_000,
            cancellationToken);
    }

    public async Task<DockerExecutionResult> ExecuteAsync(
        string workspace,
        string input,
        int timeLimitMs,
        int memoryLimitMb,
        CancellationToken cancellationToken)
    {
        var containerName =
            $"judge-{Guid.NewGuid():N}";

        var arguments = new List<string>
        {
            "run",
            "--rm",

            "--name", containerName,

            "--network", "none",

            "--memory", $"{memoryLimitMb}m",

            "--cpus", "1",

            "--pids-limit", "64",

            "--cap-drop", "ALL",

            "--security-opt", "no-new-privileges",

            "--mount",
            $"type=bind,source={workspace},target=/workspace",

            "--workdir", "/workspace",

            ImageName,

            "./Main"
        };

        try
        {
            return await RunDockerAsync(
                arguments,
                workspace,
                input,
                timeLimitMs,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning(
                "Execution timed out for container {ContainerName}",
                containerName);

            await KillContainerAsync(containerName);

            return new DockerExecutionResult(
                ExitCode: -1,
                StandardOutput: "",
                StandardError: "Time limit exceeded.",
                ExecutionTimeMs: timeLimitMs,
                TimedOut: true);
        }
    }

    private async Task<DockerExecutionResult> RunDockerAsync(
        List<string> arguments,
        string workspace,
        string? input,
        int timeoutMs,
        CancellationToken cancellationToken)
    {
        var startTime = Stopwatch.GetTimestamp();

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "docker",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var argument in arguments)
        {
            processStartInfo.ArgumentList.Add(argument);
        }

        using var process = new Process
        {
            StartInfo = processStartInfo
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
                    process.Kill(entireProcessTree: true);
                }
            }
            catch
            {
                // Process may already have exited.
            }

            throw;
        }

        var stdout =
            await process.StandardOutput.ReadToEndAsync();

        var stderr =
            await process.StandardError.ReadToEndAsync();

        var elapsed =
            Stopwatch.GetElapsedTime(startTime);

        return new DockerExecutionResult(
            ExitCode: process.ExitCode,
            StandardOutput: stdout,
            StandardError: stderr,
            ExecutionTimeMs: elapsed.Milliseconds,
            TimedOut: false);
    }

    private async Task KillContainerAsync(
        string containerName)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "docker",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            startInfo.ArgumentList.Add(
                "kill");

            startInfo.ArgumentList.Add(
                containerName);

            using var process =
                Process.Start(startInfo);

            if (process is null)
            {
                return;
            }

            await process.WaitForExitAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to kill Docker container {ContainerName}",
                containerName);
        }
    }
}
