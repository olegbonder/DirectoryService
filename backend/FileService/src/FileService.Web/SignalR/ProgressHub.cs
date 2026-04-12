using Microsoft.AspNetCore.SignalR;

namespace FileService.Web.SignalR;

public sealed class ProgressHub : Hub<IProgressClient>
{
    public Task JoinProcess(Guid mediaAssetId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, mediaAssetId.ToString());
    }
}
