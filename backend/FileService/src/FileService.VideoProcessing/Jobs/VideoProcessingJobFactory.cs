using FileService.Core.Processing;
using FileService.Domain.Assets;
using Quartz;

namespace FileService.VideoProcessing.Jobs;

public class VideoProcessingJobFactory : IProcessingJobFactory
{
    private const string JOB_GROUP = "video-processing";

    public bool CanProcess(MediaAsset mediaAsset)
    {
        return mediaAsset is VideoAsset;
    }

    public IJobDetail CreateJob(MediaAsset mediaAsset)
    {
        return JobBuilder.Create<VideoProcessingJob>()
            .WithIdentity($"video-processing-{mediaAsset.Id}", JOB_GROUP)
            .UsingJobData(VideoProcessingJob.VideoAssetIdKey.Name, mediaAsset.Id.ToString())
            .StoreDurably(false) // job delete if not triggers
            .RequestRecovery(true) // job retry if application failed
            .Build();
    }

    public ITrigger CreateTrigger(MediaAsset mediaAsset)
    {
        return TriggerBuilder.Create()
            .WithIdentity($"video-processing-trigger-{mediaAsset.Id}", JOB_GROUP)
            .StartNow()
            .Build();
    }
}