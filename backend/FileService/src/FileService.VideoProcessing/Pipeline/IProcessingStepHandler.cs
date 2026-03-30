using FileService.Domain.MediaProcessing;
using SharedKernel.Result;

namespace FileService.VideoProcessing.Pipeline
{
    public interface IProcessingStepHandler
    {
        StepType StepType { get; }

        Task<Result<ProcessingContext>> ExecuteAsync(
            ProcessingContext context,
            CancellationToken cancellationToken);
    }
}