using Core.Abstractions;
using Core.Validation;
using FileService.Contracts.Dtos.MediaAssets.AbortMultipartUpload;
using FileService.Core.Database;
using FileService.Core.FilesStorage;
using FileService.Core.Repositories;
using FileService.Domain.Assets;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace FileService.Core.Features.AbortMultipartUpload;

public sealed class AbortMultipartUploadHandler : IResultCommandHandler<AbortMultipartUploadCommand>
{
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly ILogger<AbortMultipartUploadHandler> _logger;
    private readonly IValidator<AbortMultipartUploadCommand> _validator;
    private readonly IFileStorageProvider _fileStorageProvider;
    private readonly ITransactionManager _transactionManager;

    public AbortMultipartUploadHandler(
        IMediaAssetRepository mediaAssetRepository,
        ILogger<AbortMultipartUploadHandler> logger,
        IValidator<AbortMultipartUploadCommand> validator,
        IFileStorageProvider fileStorageProvider,
        ITransactionManager transactionManager)
    {
        _mediaAssetRepository = mediaAssetRepository;
        _logger = logger;
        _validator = validator;
        _fileStorageProvider = fileStorageProvider;
        _transactionManager = transactionManager;
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

        Result abortMultipartUploadResult = await _fileStorageProvider.AbortMultipartUploadAsync(mediaAsset.RawKey, uploadId, cancellationToken);
        if (abortMultipartUploadResult.IsFailure)
            return abortMultipartUploadResult.Errors;

        mediaAsset.MarkFailed(DateTime.UtcNow);
        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Errors;

        _logger.LogInformation("Media Asset failed uploading: {MediaAssetId} with key: {StorageKey}", mediaAsset.Id, mediaAsset.RawKey);

        return Result.Success();
    }
}
