using Core.Abstractions;
using FileService.Contracts.Dtos.MediaAssets.DownloadFile;
using FileService.Contracts.Dtos.VideoProcessing;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace FileService.Core.Features.VideoProcessing;

public sealed class GetVideoProcessingProgressHandler : IQueryHandler<ProgressEventDto, ProgressEventRequest>
{
    private readonly IProgressEventQueue _progressEventQueue;
    private readonly ILogger<GetVideoProcessingProgressHandler> _logger;

    public GetVideoProcessingProgressHandler(
        IProgressEventQueue progressEventQueue,
        ILogger<GetVideoProcessingProgressHandler> logger)
    {
        _progressEventQueue = progressEventQueue;
        _logger = logger;
    }

    public Task<Result<ProgressEventDto>> Handle(ProgressEventRequest query, CancellationToken cancellationToken)
    {
        var progressEventResult = _progressEventQueue.TryGetLatest(query.VideoAssetId);
        return Task.FromResult(progressEventResult.IsFailure
            ? Result<ProgressEventDto>.Failure(progressEventResult.Errors)
            : Result<ProgressEventDto>.Success(progressEventResult.Value));
    }
}
