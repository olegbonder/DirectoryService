using Core.Abstractions;
using Core.Validation;
using FileService.Contracts.MediaAssets.StartMultiPartUpload;
using FileService.Core.FilesStorage;
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
    private readonly IS3Provider _s3Provider;
    private readonly IValidator<StartMultiPartUploadCommand> _validator;
    private readonly IChunkSizeCalculator _chunkSizeCalculator;

    public StartMultiPartUploadHandler(
        IMediaAssetRepository mediaAssetRepository,
        ILogger<StartMultiPartUploadHandler> logger,
        IS3Provider s3Provider,
        IValidator<StartMultiPartUploadCommand> validator,
        IChunkSizeCalculator chunkSizeCalculator)
    {
        _mediaAssetRepository = mediaAssetRepository;
        _logger = logger;
        _s3Provider = s3Provider;
        _validator = validator;
        _chunkSizeCalculator = chunkSizeCalculator;
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

        await _mediaAssetRepository.Add(mediaAssetResult.Value, cancellationToken);

        var mediaAsset = mediaAssetResult.Value;
        var startUploadResult = await _s3Provider.StartMultiPartUploadAsync(mediaAsset.RawKey, mediaData, cancellationToken);
        if (startUploadResult.IsFailure)
            return startUploadResult.Errors;

        string uploadId = startUploadResult.Value;
        var chunksUploadUrlsResult = await _s3Provider.GenerateAllChunksUploadUrlsAsync(
            mediaAsset.RawKey,
            uploadId,
            totalChunks,
            cancellationToken);
        if (chunksUploadUrlsResult.IsFailure)
            return chunksUploadUrlsResult.Errors;

        _logger.LogInformation("Media Asset started uploading: {MediaAssetId} with key: {StorageKey}", mediaAsset.Id, mediaAsset.RawKey);

        return new StartMultiPartUploadResponse(
            mediaAsset.Id,
            uploadId,
            chunksUploadUrlsResult.Value,
            chunkSize);
    }
}
