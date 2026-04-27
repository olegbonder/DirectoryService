using System.Text.Json;
using FileService.VideoProcessing.Pipeline;
using Microsoft.Extensions.Logging;
using Quartz;

namespace FileService.VideoProcessing.Jobs;

public class VideoProcessingJob : IJob
{
    public static readonly JobKey VideoAssetIdKey = new("VideoAssetId");
    private readonly ILogger<VideoProcessingJob> _logger;
    private readonly IProcessingPipeline _videoProcessingService;

    public VideoProcessingJob(
        ILogger<VideoProcessingJob> logger,
        IProcessingPipeline videoProcessingService)
    {
        _logger = logger;
        _videoProcessingService = videoProcessingService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var videoAssetIdStr = context.JobDetail.JobDataMap.GetString(VideoAssetIdKey.Name);
        var videoAssetId = Guid.Parse(videoAssetIdStr);

        _logger.LogInformation("Starting video processing job for asset {VideoAssetId}", videoAssetId);

        var result = await _videoProcessingService.ProcessAllStepsAsync(videoAssetId, context.CancellationToken);
        if (result.IsFailure)
        {
            var errors = result.Errors.Select(e => new { e.Code, e.Message });
            string errorMessage = JsonSerializer.Serialize(errors);
            _logger.LogError(
                "Video processing job failed for asset {VideoAssetId}: {Error}",
                videoAssetId,
                errorMessage);

            throw new JobExecutionException(refireImmediately: false);
        }

        _logger.LogInformation("Video processing completed successfully for asset {VideoAssetId}", videoAssetId);
    }
}
