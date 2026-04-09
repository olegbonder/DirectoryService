using Core.Abstractions;
using Core.Validation;
using FileService.Contracts.Dtos.MediaAssets.StartMultiPartUpload;
using FileService.Core.Database;
using FileService.Core.FilesStorage;
using FileService.Core.Messaging;
using FileService.Domain;
using FileService.Domain.Assets;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace FileService.Core.Features.StartMultiPartUpload;

public sealed class StartMultiPartUploadHandler : ICommandHandler<StartMultiPartUploadResponse, StartMultiPartUploadCommand>
{
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly ILogger<StartMultiPartUploadHandler> _logger;
    private readonly IFileStorageProvider _fileStorageProvider;
    private readonly IValidator<StartMultiPartUploadCommand> _validator;
    private readonly IChunkSizeCalculator _chunkSizeCalculator;
    private readonly IAssetCreatedEventPublisher _assetCreatedEventPublisher;
    private readonly ITransactionManager _transactionManager;

    public StartMultiPartUploadHandler(
        IMediaAssetRepository mediaAssetRepository,
        ILogger<StartMultiPartUploadHandler> logger,
        IFileStorageProvider fileStorageProvider,
        IValidator<StartMultiPartUploadCommand> validator,
        IChunkSizeCalculator chunkSizeCalculator,
        IAssetCreatedEventPublisher assetCreatedEventPublisher,
        ITransactionManager transactionManager)
    {
        _mediaAssetRepository = mediaAssetRepository;
        _logger = logger;
        _fileStorageProvider = fileStorageProvider;
        _validator = validator;
        _chunkSizeCalculator = chunkSizeCalculator;
        _assetCreatedEventPublisher = assetCreatedEventPublisher;
        _transactionManager = transactionManager;
    }

    public async Task<Result<StartMultiPartUploadResponse>> Handle(StartMultiPartUploadCommand command, CancellationToken cancellationToken)
    {
        var validResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validResult.IsValid == false)
        {
            return validResult.ToList();
        }

        var request = command.Request;
        var fileName = FileName.Create(request.FileName).Value;
        var contentType = ContentType.Create(request.ContentType).Value;

        var chunkCalculationResult = _chunkSizeCalculator.CalculateChunkSize(request.Size);
        if (chunkCalculationResult.IsFailure)
            return chunkCalculationResult.Errors;

        (int chunkSize, int totalChunks) = chunkCalculationResult.Value;

        var mediaDataResult = MediaData.Create(fileName, contentType, request.Size, totalChunks);
        if (mediaDataResult.IsFailure)
            return mediaDataResult.Errors;

        var mediaData = mediaDataResult.Value;

        var owner = MediaOwner.Create(request.Context, request.ContextId).Value;

        var mediaAssetResult = MediaAsset.CreateForUpload(mediaData, request.AssetType.ToAssetType(), owner);
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Errors;

        var mediaAsset = mediaAssetResult.Value;
        await _mediaAssetRepository.Add(mediaAsset, cancellationToken);

        var startUploadResult = await _fileStorageProvider.StartMultiPartUploadAsync(mediaAsset.UploadKey!, mediaData, cancellationToken);
        if (startUploadResult.IsFailure)
            return startUploadResult.Errors;

        string uploadId = startUploadResult.Value;
        var chunksUploadUrlsResult = await _fileStorageProvider.GenerateAllChunksUploadUrlsAsync(
            mediaAsset.UploadKey!,
            uploadId,
            totalChunks,
            cancellationToken);
        if (chunksUploadUrlsResult.IsFailure)
            return chunksUploadUrlsResult.Errors;

        var publishResult = await _assetCreatedEventPublisher.PublishAsync(mediaAsset);
        if (publishResult.IsFailure)
            return publishResult.Errors;

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Errors;

        _logger.LogInformation("Media Asset started uploading: {MediaAssetId} with key: {StorageKey}", mediaAsset.Id, mediaAsset.RawKey);

        return new StartMultiPartUploadResponse(
            mediaAsset.Id,
            uploadId,
            chunksUploadUrlsResult.Value,
            chunkSize);
    }
}
