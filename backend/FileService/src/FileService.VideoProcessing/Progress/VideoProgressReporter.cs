using FileService.Contracts.Dtos.VideoProcessing;
using FileService.Core;
using FileService.Domain.MediaProcessing;
using Microsoft.Extensions.Logging;

namespace FileService.VideoProcessing.Progress;

public sealed class VideoProgressReporter : IVideoProgressReporter
{
    private readonly IProgressEventQueue _progressEventQueue;
    private readonly ILogger<VideoProgressReporter> _logger;

    public VideoProgressReporter(
        IProgressEventQueue progressEventQueue,
        ILogger<VideoProgressReporter> logger)
    {
        _progressEventQueue = progressEventQueue;
        _logger = logger;
    }

    public void Publish(VideoProcess videoProcess)
    {
        var progressEvent = ProgressEventDto.FromVideoProcess(videoProcess);
        var enqueueResult = _progressEventQueue.TryWriteQueue(progressEvent);

        if (enqueueResult.IsFailure)
        {
            _logger.LogWarning(
                "Progress event queue is full and event for media asset {MediaAssetId} was dropped.",
                videoProcess.Id);
        }
    }
}
