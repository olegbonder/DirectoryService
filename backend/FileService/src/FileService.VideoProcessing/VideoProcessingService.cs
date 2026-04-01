using FileService.VideoProcessing.Pipeline;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace FileService.VideoProcessing;

public class VideoProcessingService : IVideoProcessingService
{
    private readonly ILogger<VideoProcessingService> _logger;
    private readonly IProcessingPipeline _pipeline;

    public VideoProcessingService(
        ILogger<VideoProcessingService> logger,
        IProcessingPipeline pipeline)
    {
        _logger = logger;
        _pipeline = pipeline;
    }

    public Task<Result> ProcessVideoAsync(
        Guid videoAssetId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting video processing for asset {VideoAssetId}", videoAssetId);

        var pipelineResult = _pipeline
            .ProcessAllStepsAsync(videoAssetId, cancellationToken);

        return pipelineResult;
    }
}
