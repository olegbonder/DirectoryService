using Core.Validation;
using FileService.Contracts.MediaAssets.AbortMultipartUpload;
using FileService.Core.FilesStorage;
using FileService.Domain.Assets;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace FileService.Core.Features.AbortMultipartUpload;

public sealed class AbortMultipartUploadHandler
{
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly ILogger<AbortMultipartUploadHandler> _logger;
    private readonly IValidator<AbortMultipartUploadCommand> _validator;
    private readonly IS3Provider _s3Provider;

    public AbortMultipartUploadHandler(
        IMediaAssetRepository mediaAssetRepository,
        ILogger<AbortMultipartUploadHandler> logger,
        IValidator<AbortMultipartUploadCommand> validator,
        IS3Provider s3Provider)
    {
        _mediaAssetRepository = mediaAssetRepository;
        _logger = logger;
        _validator = validator;
        _s3Provider = s3Provider;
    }

    public async Task<Result> Handle(AbortMultipartUploadCommand command, CancellationToken cancellationToken)
    {
        var validResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validResult.IsValid == false)
        {
            return validResult.ToList();
        }

        AbortMultipartUploadRequest request = command.Request;
        Guid mediaAssetId = request.MediaAssetId;
        string uploadId = request.UploadId;

        Result<MediaAsset> mediaAssetResult = await _mediaAssetRepository.GetById(mediaAssetId, cancellationToken);
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Errors;

        MediaAsset mediaAsset = mediaAssetResult.Value;

        Result abortMultipartUploadResult = await _s3Provider.AbortMultipartUploadAsync(mediaAsset.RawKey, uploadId, cancellationToken);
        if (abortMultipartUploadResult.IsFailure)
            return abortMultipartUploadResult.Errors;

        mediaAsset.MarkFailed(DateTime.UtcNow);
        await _mediaAssetRepository.SaveChanges(cancellationToken);
        _logger.LogInformation("Media Asset failed uploading: {MediaAssetId} with key: {StorageKey}", mediaAsset.Id, mediaAsset.RawKey);

        return Result.Success();
    }
}
