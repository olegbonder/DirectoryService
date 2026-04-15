using Core.Abstractions;
using Core.Validation;
using FileService.Contracts.Dtos.MediaAssets;
using FileService.Contracts.Dtos.MediaAssets.CompleteMultiPartUpload;
using FileService.Core.Database;
using FileService.Core.Features.CompleteMultipartUpload;
using FileService.Core.FilesStorage;
using FileService.Core.Repositories;
using FileService.Domain.Assets;
using FileService.Domain.MediaProcessing;
using FileService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace FileService.Core.Features.CompleteMultiPartUpload;

public sealed class CompleteMultiPartUploadHandler : ICommandHandler<MediaAssetResponse, CompleteMultipartUploadCommand>
{
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly IVideoProcessingRepository _videoProcessingRepository;
    private readonly IVideoProcessingScheduler _videoProcessingScheduler;
    private readonly ILogger<CompleteMultiPartUploadHandler> _logger;
    private readonly IValidator<CompleteMultipartUploadCommand> _validator;
    private readonly IFileStorageProvider _fileStorageProvider;
    private readonly ITransactionManager _transactionManager;

    public CompleteMultiPartUploadHandler(
        IMediaAssetRepository mediaAssetRepository,
        IVideoProcessingRepository videoProcessingRepository,
        IVideoProcessingScheduler videoProcessingScheduler,
        ILogger<CompleteMultiPartUploadHandler> logger,
        IValidator<CompleteMultipartUploadCommand> validator,
        IFileStorageProvider fileStorageProvider,
        ITransactionManager transactionManager)
    {
        _mediaAssetRepository = mediaAssetRepository;
        _videoProcessingRepository = videoProcessingRepository;
        _videoProcessingScheduler = videoProcessingScheduler;
        _logger = logger;
        _validator = validator;
        _fileStorageProvider = fileStorageProvider;
        _transactionManager = transactionManager;
    }

    public async Task<Result<MediaAssetResponse>> Handle(CompleteMultipartUploadCommand command, CancellationToken cancellationToken)
    {
        var validResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validResult.IsValid == false)
        {
            return validResult.ToList();
        }

        CompleteMultiPartUploadRequest request = command.Request;
        Guid mediaAssetId = request.MediaAssetId;
        string uploadId = request.UploadId;
        IReadOnlyList<PartEtagDto> partETags = request.PartETags;

        Result<MediaAsset> mediaAssetResult = await _mediaAssetRepository.GetById(mediaAssetId, cancellationToken);
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Errors;

        MediaAsset mediaAsset = mediaAssetResult.Value;

        if (mediaAsset.MediaData.ExpectedChunksCount != partETags.Count)
            return MediaAssetErrors.ExpectedChunksCount();

        Result completeMultiPartUploadResult = await _fileStorageProvider.CompleteMultiPartUploadAsync(
            mediaAsset.UploadKey,
            uploadId,
            partETags,
            cancellationToken);
        if (completeMultiPartUploadResult.IsFailure)
            return completeMultiPartUploadResult.Errors;

        mediaAsset.MarkUploaded(DateTime.UtcNow);
        var markUploadedSaveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (markUploadedSaveResult.IsFailure)
            return markUploadedSaveResult.Errors;

        _logger.LogInformation("Media Asset completed uploading: {MediaAssetId} with key: {StorageKey}", mediaAsset.Id, mediaAsset.RawKey);

        if (mediaAsset is VideoAsset videoAsset)
        {
            var createProcessResult = await CreateVideoProcessAsync(videoAsset, cancellationToken);
            if (createProcessResult.IsFailure)
                return createProcessResult.Errors;

            _logger.LogInformation("VideoProcess created for video asset {VideoAssetId}", videoAsset.Id);

            var scheduleProcessResult = await _videoProcessingScheduler.ScheduleProcessingAsync(videoAsset.Id, cancellationToken);
            if (scheduleProcessResult.IsFailure)
                return scheduleProcessResult.Errors;

            _logger.LogInformation("Video processing scheduled for video asset {VideoAssetId}", videoAsset.Id);
        }

        return new MediaAssetResponse(mediaAsset.Id);
    }

    private async Task<Result> CreateVideoProcessAsync(VideoAsset videoAsset, CancellationToken cancellationToken)
    {
        var steps = VideoProcess.CreateProcessingSteps();
        var videoProcessResult = VideoProcess.Create(
            videoAsset.Id,
            videoAsset.RawKey,
            null,
            steps);

        if (videoProcessResult.IsFailure)
            return videoProcessResult.Errors;

        _videoProcessingRepository.Add(videoProcessResult.Value);
        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Errors;

        return Result.Success();
    }
}
