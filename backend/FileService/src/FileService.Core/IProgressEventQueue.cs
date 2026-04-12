using System.Threading.Channels;
using FileService.Contracts.Dtos.VideoProcessing;
using SharedKernel.Result;

namespace FileService.Core;

public interface IProgressEventQueue
{
    ChannelReader<ProgressEventDto> Reader { get; }

    Result TryWriteQueue(ProgressEventDto progressEvent);

    Result<ProgressEventDto> TryGetLatest(Guid videoAssetId);
}
