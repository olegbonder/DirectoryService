using Core.Abstractions;
using Core.Validation;
using FileService.Contracts.Dtos.MediaAssets;
using FileService.Contracts.Dtos.MediaAssets.CompleteMultiPartUpload;
using FileService.Core.Features.CompleteMultipartUpload;
using FileService.Core.FilesStorage;
using FileService.Domain.Assets;
using FileService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace FileService.Core.Features.CompleteMultiPartUpload;

public sealed class CompleteMultiPartUploadHandler : ICommandHandler<MediaAssetResponse, CompleteMultipartUploadCommand>
{
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly ILogger<CompleteMultiPartUploadHandler> _logger;
    private readonly IValidator<CompleteMultipartUploadCommand> _validator;
    private readonly IS3Provider _s3Provider;

    public CompleteMultiPartUploadHandler(
        IMediaAssetRepository mediaAssetRepository,
        ILogger<CompleteMultiPartUploadHandler> logger,
        IValidator<CompleteMultipartUploadCommand> validator,
        IS3Provider s3Provider)
    {
        _mediaAssetRepository = mediaAssetRepository;
        _logger = logger;
        _validator = validator;
        _s3Provider = s3Provider;
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

        Result completeMultiPartUploadResult = await _s3Provider.CompleteMultiPartUploadAsync(
            mediaAsset.RawKey,
            uploadId,
            partETags,
            cancellationToken);
        if (completeMultiPartUploadResult.IsFailure)
            return completeMultiPartUploadResult.Errors;

        mediaAsset.MarkUploaded(DateTime.UtcNow);
        await _mediaAssetRepository.SaveChanges(cancellationToken);
        _logger.LogInformation("Media Asset completed uploading: {MediaAssetId} with key: {StorageKey}", mediaAsset.Id, mediaAsset.RawKey);

        return new MediaAssetResponse(mediaAsset.Id);
    }
}
