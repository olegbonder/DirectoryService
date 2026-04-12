using FileService.Core;
using Microsoft.AspNetCore.SignalR;

namespace FileService.Web.SignalR;

public sealed class ProgressConsumer : BackgroundService
{
    private readonly IProgressEventQueue _progressEventQueue;
    private readonly IHubContext<ProgressHub, IProgressClient> _hubContext;
    private readonly ILogger<ProgressConsumer> _logger;

    public ProgressConsumer(
        IProgressEventQueue progressEventQueue,
        IHubContext<ProgressHub, IProgressClient> hubContext,
        ILogger<ProgressConsumer> logger)
    {
        _progressEventQueue = progressEventQueue;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var progressEvent in _progressEventQueue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await _hubContext.Clients
                    .Group(progressEvent.MediaAssetId.ToString())
                    .ReceiveProgress(progressEvent);
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
