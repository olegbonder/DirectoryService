using SharedKernel.Result;

namespace FileService.VideoProcessing.ProcessRunner
{
    public interface IProcessRunner
    {
        Task<Result<ProcessResult>> RunAsync(
            ProcessCommand command,
            Action<string>? onOutput = null,
            CancellationToken cancellationToken = default);
    }
}