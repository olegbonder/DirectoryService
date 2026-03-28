using SharedKernel.Result;

namespace FileService.VideoProcessing.Pipeline
{
    public interface IProcessingPipeline
    {
        Task<Result> ProcessAllStepsAsync(Guid videoAssetId, CancellationToken cancellationToken);
    }
}