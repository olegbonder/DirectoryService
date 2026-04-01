using SharedKernel.Result;

namespace FileService.VideoProcessing;

public interface IVideoProcessingService
{
    Task<Result> ProcessVideoAsync(Guid videoAssetId, CancellationToken cancellationToken);
}