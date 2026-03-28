using System.Diagnostics;
using System.Text;
using FileService.Domain;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace FileService.VideoProcessing.ProcessRunner
{
    public class ProcessRunner : IProcessRunner
    {
        private readonly ILogger<ProcessRunner> _logger;

        public ProcessRunner(ILogger<ProcessRunner> logger)
        {
            _logger = logger;
        }

        public async Task<Result<ProcessResult>> RunAsync(
            ProcessCommand command,
            Action<string>? onOutput = null,
            CancellationToken cancellationToken = default)
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo()
            {
                FileName = command.ExecutableFile,
                Arguments = command.NormalizedArguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (_, args) =>
            {
                if (args.Data is null)
                    return;

                outputBuilder.AppendLine(args.Data);
                onOutput?.Invoke(args.Data);
            };

            process.ErrorDataReceived += (_, args) =>
            {
                if (args.Data is null)
                    return;

                errorBuilder.AppendLine(args.Data);
                onOutput?.Invoke(args.Data);
            };

            _logger.LogInformation("Starting process {ProcessName} {Arguments}.", command.ExecutableFile, command.Arguments);

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            try
            {
                await process.WaitForExitAsync(cancellationToken);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Process {ProcessName} {Arguments} was canceled.", command.ExecutableFile, command.Arguments);
                return Error.Failure("process.canceled", "Process was canceled.");
            }

            var result = new ProcessResult(process.ExitCode, outputBuilder.ToString(), errorBuilder.ToString());

            if (process.ExitCode != 0)
            {
                _logger.LogError(
                    "Process {ProcessName} {Arguments} exited with code {ExitCode}.",
                    command.ExecutableFile,
                    command.Arguments,
                    process.ExitCode);
                return FileErrors.ProcessFailed();
            }

            return result;
        }
    }
}