using Core.Abstractions;
using Core.Validation;
using FileService.Core.Database;
using FileService.Core.FilesStorage;
using FileService.Core.Repositories;
using FileService.Domain;
using FileService.Domain.Assets;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace FileService.Core.Features.UploadFile;

public class UploadFileHandler : ICommandHandler<Guid, UploadFileCommand>
{
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly IFileStorageProvider _fileStorageProvider;
    private readonly IValidator<UploadFileCommand> _validator;
    private readonly ILogger<UploadFileHandler> _logger;
    private readonly ITransactionManager _transactionManager;

    public UploadFileHandler(
        IMediaAssetRepository mediaAssetRepository,
        IFileStorageProvider fileStorageProvider,
        IValidator<UploadFileCommand> validator,
        ILogger<UploadFileHandler> logger,
        ITransactionManager transactionManager)
    {
        _mediaAssetRepository = mediaAssetRepository;
        _fileStorageProvider = fileStorageProvider;
        _validator = validator;
        _logger = logger;
        _transactionManager = transactionManager;
    }

    public async Task<Result<Guid>> Handle(UploadFileCommand command, CancellationToken cancellationToken)
    {
        var validResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validResult.IsValid == false)
        {
            return validResult.ToList();
        }

        var request = command.Request;
        var assetType = request.AssetType.ToAssetType();
        var file = request.File;
        var fileName = FileName.Create(file.FileName).Value;
        var contentType = ContentType.Create(file.ContentType).Value;
        long size = file.Length;
        var mediaDataResult = MediaData.Create(fileName, contentType, size, 1);
        if (mediaDataResult.IsFailure)
            return mediaDataResult.Errors;

        var mediaData = mediaDataResult.Value;
        var ownerResult = MediaOwner.Create(request.Context, request.ContextId);
        if (ownerResult.IsFailure)
            return ownerResult.Errors;

        var owner = ownerResult.Value;
        var mediaAssetResult = MediaAsset.CreateForUpload(mediaData, assetType, owner);
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Errors;

        var mediaAsset = mediaAssetResult.Value;
        var addResult = await _mediaAssetRepository.Add(mediaAsset, cancellationToken);
        if (addResult.IsFailure)
            return addResult.Errors;

        var uploadResult = await _fileStorageProvider.UploadFileAsync(
            mediaAsset.UploadKey!,
            file.OpenReadStream(),
            mediaData.ContentType.Value,
            cancellationToken);
        if (uploadResult.IsFailure)
        {
            mediaAsset.MarkFailed(DateTime.UtcNow);
            var uploadSaveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (uploadSaveResult.IsFailure)
                return uploadSaveResult.Errors;

            return uploadResult.Errors;
        }

        mediaAsset.MarkUploaded(DateTime.UtcNow);
        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Errors;

        _logger.LogInformation($"Uploaded file {file.Name}");

        return addResult.Value;
    }
}