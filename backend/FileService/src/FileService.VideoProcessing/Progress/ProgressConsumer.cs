using FileService.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FileService.VideoProcessing.Progress;

public sealed class ProgressConsumer : BackgroundService
{
    private readonly IProgressEventQueue _progressEventQueue;
    private readonly IProgressNotifier _progressNotifier;
    private readonly ILogger<ProgressConsumer> _logger;

    public ProgressConsumer(
        IProgressEventQueue progressEventQueue,
        IProgressNotifier progressNotifier,
        ILogger<ProgressConsumer> logger)
    {
        _progressEventQueue = progressEventQueue;
        _progressNotifier = progressNotifier;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var progressEvent in _progressEventQueue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await _progressNotifier.NotifyProgressAsync(progressEvent, stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Failed to publish progress event for media asset {MediaAssetId}",
                    progressEvent.MediaAssetId);
            }
        }
    }
}
