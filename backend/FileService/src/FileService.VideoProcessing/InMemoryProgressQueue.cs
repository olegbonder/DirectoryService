using System.Collections.Concurrent;
using System.Threading.Channels;
using FileService.Contracts.Dtos.VideoProcessing;
using FileService.Core;
using SharedKernel.Result;

namespace FileService.VideoProcessing;

public sealed class InMemoryProgressQueue : IProgressEventQueue
{
    private readonly Channel<ProgressEventDto> _channel;
    private readonly ConcurrentDictionary<Guid, ProgressEventDto> _latestProgress = new();

    public InMemoryProgressQueue()
    {
        var options = new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = false,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        };

        _channel = Channel.CreateBounded<ProgressEventDto>(options);
    }

    public ChannelReader<ProgressEventDto> Reader => _channel.Reader;

    public Result TryWriteQueue(ProgressEventDto progressEvent)
    {
        try
        {
            _latestProgress.AddOrUpdate(progressEvent.MediaAssetId, progressEvent, (_, _) => progressEvent);
        }
        catch (Exception ex)
        {
            return Error.Failure("progress_queue.update_failed", $"Failed to update latest progress for media asset {progressEvent.MediaAssetId}.");
        }

        return _channel.Writer.TryWrite(progressEvent)
            ? Result.Success()
            : Result.Failure(Error.Failure("progress_queue.full", $"Progress queue is full for media asset {progressEvent.MediaAssetId}."));
    }

    public Result<ProgressEventDto> TryGetLatest(Guid videoAssetId)
    {
        if (_latestProgress.TryGetValue(videoAssetId, out var progressEvent))
        {
            return Result<ProgressEventDto>.Success(progressEvent);
        }

        return Result<ProgressEventDto>.Failure(
            Error.NotFound("video_processing_progress.not.found", $"Progress for video asset {videoAssetId} not found"));
    }
}
