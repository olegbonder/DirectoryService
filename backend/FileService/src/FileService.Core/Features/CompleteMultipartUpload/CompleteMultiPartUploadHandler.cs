using Core.Abstractions;
using Core.Validation;
using FileService.Contracts.Dtos.MediaAssets;
using FileService.Contracts.Dtos.MediaAssets.CompleteMultiPartUpload;
using FileService.Core.Database;
using FileService.Core.Features.CompleteMultipartUpload;
using FileService.Core.FilesStorage;
using FileService.Core.Processing;
using FileService.Core.Repositories;
using FileService.Domain.Assets;
using FileService.Domain.MediaProcessing;
using FileService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Quartz;
using SharedKernel.Result;

namespace FileService.Core.Features.CompleteMultiPartUpload;

public sealed class CompleteMultiPartUploadHandler : ICommandHandler<MediaAssetResponse, CompleteMultipartUploadCommand>
{
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly IVideoProcessingRepository _videoProcessingRepository;
    private readonly ILogger<CompleteMultiPartUploadHandler> _logger;
    private readonly IValidator<CompleteMultipartUploadCommand> _validator;
    private readonly IFileStorageProvider _fileStorageProvider;
    private readonly ITransactionManager _transactionManager;
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly IEnumerable<IProcessingJobFactory> _processingJobFactories;

    public CompleteMultiPartUploadHandler(
        IMediaAssetRepository mediaAssetRepository,
        IVideoProcessingRepository videoProcessingRepository,
        ILogger<CompleteMultiPartUploadHandler> logger,
        IValidator<CompleteMultipartUploadCommand> validator,
        IFileStorageProvider fileStorageProvider,
        ITransactionManager transactionManager,
        ISchedulerFactory schedulerFactory,
        IEnumerable<IProcessingJobFactory> processingJobFactories)
    {
        _mediaAssetRepository = mediaAssetRepository;
        _videoProcessingRepository = videoProcessingRepository;
        _logger = logger;
        _validator = validator;
        _fileStorageProvider = fileStorageProvider;
        _transactionManager = transactionManager;
        _schedulerFactory = schedulerFactory;
        _processingJobFactories = processingJobFactories;
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

        try
        {
            var transaction = await _transactionManager.BeginTransactionAsync(cancellationToken);

            if (completeMultiPartUploadResult.IsFailure)
            {
                mediaAsset.MarkFailed(DateTime.UtcNow);
                var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
                if (saveResult.IsFailure)
                    return saveResult.Errors;
                return completeMultiPartUploadResult.Errors;
            }

            mediaAsset.MarkUploaded(DateTime.UtcNow);
            var markUploadedSaveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (markUploadedSaveResult.IsFailure)
                return markUploadedSaveResult.Errors;

            _logger.LogInformation("Media Asset completed uploading: {MediaAssetId} with key: {StorageKey}", mediaAsset.Id, mediaAsset.RawKey);

            if (mediaAsset.RequiredProcessing())
            {
                var createProcessResult = await CreateVideoProcessAsync(mediaAsset, cancellationToken);
                if (createProcessResult.IsFailure)
                    return createProcessResult.Errors;

                _logger.LogInformation("VideoProcess created for video asset {VideoAssetId}", mediaAsset.Id);

                var scheduleProcessResult = await ScheduleProcessingAsync(mediaAsset, cancellationToken);
                if (scheduleProcessResult.IsFailure)
                    return scheduleProcessResult.Errors;
            }
            else
            {
                var markReadyResult = mediaAsset.MarkReady(mediaAsset.FinalKey, DateTime.UtcNow);
                if (markReadyResult.IsFailure)
                    return markReadyResult.Errors;

                _logger.LogInformation("MediaAssetId: {MediaAssetId} does not require processing", mediaAsset.Id);
            }

            var saveFinalResult = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (saveFinalResult.IsFailure)
                return saveFinalResult.Errors;

            await _transactionManager.CommitTransactionAsync(cancellationToken);
            return new MediaAssetResponse(mediaAsset.Id);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error completing multipart upload for MediaAssetId: {MediaAssetId}", mediaAsset.Id);
            return GeneralErrors.Failure("Error completing multipart upload");
        }
    }

    private async Task<Result> CreateVideoProcessAsync(MediaAsset mediaAsset, CancellationToken cancellationToken)
    {
        var steps = VideoProcess.CreateProcessingSteps();
        var videoProcessResult = VideoProcess.Create(
            mediaAsset.Id,
            mediaAsset.RawKey,
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

    private async Task<Result> ScheduleProcessingAsync(MediaAsset mediaAsset, CancellationToken cancellationToken)
    {
        var factory = _processingJobFactories.FirstOrDefault(f => f.CanProcess(mediaAsset));
        if (factory == null)
        {
            _logger.LogError("No processing job available for mediaAssetId: {MediaAssetId}", mediaAsset.Id);
            return GeneralErrors.Failure("No processing job factory found");
        }

        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

        var job = factory.CreateJob(mediaAsset);
        var trigger = factory.CreateTrigger(mediaAsset);

        await scheduler.ScheduleJob(job, trigger, cancellationToken);

        _logger.LogInformation("Scheduled processing job completed. MediaAssetId: {MediaAssetId}", mediaAsset.Id);

        return Result.Success();
    }
}
