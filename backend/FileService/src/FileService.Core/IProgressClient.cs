using FileService.Contracts.Dtos.VideoProcessing;

namespace FileService.Core;

public interface IProgressClient
{
    Task ReceiveProgress(ProgressEventDto progressEvent);
}
