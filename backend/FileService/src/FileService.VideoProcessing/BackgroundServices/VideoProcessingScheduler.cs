using FileService.Core;
using Microsoft.Extensions.Logging;
using Quartz;
using SharedKernel.Result;

namespace FileService.VideoProcessing.BackgroundServices;

public class VideoProcessingScheduler : IVideoProcessingScheduler
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly ILogger<VideoProcessingScheduler> _logger;

    public VideoProcessingScheduler(
        ISchedulerFactory schedulerFactory,
        ILogger<VideoProcessingScheduler> logger)
    {
        _schedulerFactory = schedulerFactory;
        _logger = logger;
    }

    public async Task<Result> ScheduleProcessingAsync(Guid videoAssetId, CancellationToken cancellationToken = default)
    {
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

        var jobKey = new JobKey($"VideoProcessing-{videoAssetId}", "video-processing");

        // Проверяем, существует ли уже job
        if (await scheduler.CheckExists(jobKey, cancellationToken))
        {
            _logger.LogWarning("Job for video asset {VideoAssetId} already exists, skipping scheduling", videoAssetId);
            return Error.Conflict("video_asset.job", $"Schedule job exists for video_asset: {videoAssetId}");
        }

        var jobDataMap = new JobDataMap
        {
            { "VideoAssetId", videoAssetId }
        };

        var job = JobBuilder.Create<VideoProcessingJob>()
            .WithIdentity(jobKey)
            .UsingJobData(jobDataMap)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"VideoProcessing-Trigger-{videoAssetId}", "video-processing")
            .StartNow()
            .Build();

        await scheduler.ScheduleJob(job, trigger, cancellationToken);

        _logger.LogInformation("Scheduled video processing job for asset {VideoAssetId}", videoAssetId);

        return Result.Success();
    }
}
