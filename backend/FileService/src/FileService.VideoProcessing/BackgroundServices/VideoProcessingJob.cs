using System.Text.Json;
using FileService.VideoProcessing.Pipeline;
using Microsoft.Extensions.Logging;
using Quartz;

namespace FileService.VideoProcessing.BackgroundServices;

public class VideoProcessingJob : IJob
{
    private readonly ILogger<VideoProcessingJob> _logger;
    private readonly IProcessingPipeline _pipeline;

    public VideoProcessingJob(
        ILogger<VideoProcessingJob> logger,
        IProcessingPipeline pipeline)
    {
        _logger = logger;
        _pipeline = pipeline;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var videoAssetId = context.JobDetail.JobDataMap.GetGuid("VideoAssetId");

        _logger.LogInformation("Starting video processing job for asset {VideoAssetId}", videoAssetId);

        var result = await _pipeline.ProcessAllStepsAsync(videoAssetId, context.CancellationToken);
        if (result.IsFailure)
        {
            var errors = result.Errors.Select(e => new { e.Code, e.Message });
            string errorMessage = JsonSerializer.Serialize(errors);
            _logger.LogError(
                "Video processing job failed for asset {VideoAssetId}: {Error}",
                videoAssetId,
                errorMessage);
        }

        _logger.LogInformation("Video processing completed successfully for asset {VideoAssetId}", videoAssetId);
    }
}
