using FileService.Contracts.Dtos.VideoProcessing;

namespace FileService.Core;

public interface IProgressNotifier
{
    Task NotifyProgressAsync(ProgressEventDto progressEvent, CancellationToken cancellationToken = default);
}
