using FileService.Contracts.Dtos.VideoProcessing;

namespace FileService.Web.SignalR;

public interface IProgressClient
{
    Task ReceiveProgress(ProgressEventDto progressEvent);
}
