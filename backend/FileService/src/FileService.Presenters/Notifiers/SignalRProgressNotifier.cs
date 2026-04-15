using FileService.Contracts.Dtos.VideoProcessing;
using FileService.Core;
using FileService.Presenters.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace FileService.Presenters.Notifiers;

public sealed class SignalRProgressNotifier : IProgressNotifier
{
    private readonly IHubContext<ProgressHub, IProgressClient> _hubContext;

    public SignalRProgressNotifier(IHubContext<ProgressHub, IProgressClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyProgressAsync(ProgressEventDto progressEvent, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .Group(progressEvent.MediaAssetId.ToString())
            .ReceiveProgress(progressEvent);
    }
}
