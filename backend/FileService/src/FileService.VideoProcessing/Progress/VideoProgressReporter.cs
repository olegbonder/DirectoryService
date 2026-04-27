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
        var progressEvent = FromVideoProcess(videoProcess);
        var enqueueResult = _progressEventQueue.TryWriteQueue(progressEvent);

        if (enqueueResult.IsFailure)
        {
            _logger.LogWarning(
                "Progress event queue is full and event for media asset {MediaAssetId} was dropped.",
                videoProcess.Id);
        }
    }

    private static ProgressEventDto FromVideoProcess(VideoProcess videoProcess)
    {
        string? errorMessage = videoProcess.ErrorMessage;
        var failedStep = videoProcess.Steps.FirstOrDefault(x => x.Status == VideoProcessStatus.FAILED);
        string? error = errorMessage ?? failedStep?.Error;

        return new ProgressEventDto(
            videoProcess.Id,
            NormalizeStatus(videoProcess.Status),
            videoProcess.TotalProgress,
            videoProcess.CurrentStepOrder,
            videoProcess.CurrentStepName,
            videoProcess.Steps.Count,
            error,
            NormalizeErrorCode(error),
            DateTime.UtcNow);
    }

    private static string NormalizeStatus(VideoProcessStatus status)
    {
        return status.ToString().ToLowerInvariant();
    }

    private static string? NormalizeErrorCode(string? error)
    {
        if (string.IsNullOrWhiteSpace(error))
            return null;

        if (error.Trim().Contains('.'))
            return error;

        return null;
    }
}
