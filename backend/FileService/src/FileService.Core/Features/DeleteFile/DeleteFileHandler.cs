using Core.Abstractions;
using Core.Validation;
using FileService.Contracts.Dtos.MediaAssets;
using FileService.Core.Database;
using FileService.Core.FilesStorage;
using FileService.Domain;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace FileService.Core.Features.DeleteFile;

public class DeleteFileHandler : ICommandHandler<MediaAssetResponse, DeleteFileCommand>
{
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly IS3Provider _s3Provider;
    private readonly IValidator<DeleteFileCommand> _validator;
    private readonly ILogger<DeleteFileHandler> _logger;
    private readonly ITransactionManager _transactionManager;

    public DeleteFileHandler(
        IMediaAssetRepository mediaAssetRepository,
        IS3Provider s3Provider,
        IValidator<DeleteFileCommand> validator,
        ILogger<DeleteFileHandler> logger,
        ITransactionManager transactionManager)
    {
        _mediaAssetRepository = mediaAssetRepository;
        _s3Provider = s3Provider;
        _validator = validator;
        _logger = logger;
        _transactionManager = transactionManager;
    }

    public async Task<Result<MediaAssetResponse>> Handle(DeleteFileCommand command, CancellationToken cancellationToken)
    {
        var validResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validResult.IsValid == false)
        {
            return validResult.ToList();
        }

        var mediaAssetId = command.MediaAssetId;
        var mediaAssetResult = await _mediaAssetRepository.GetById(mediaAssetId, cancellationToken);
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Errors;

        var mediaAsset = mediaAssetResult.Value;

        var deleteKeys = new List<StorageKey> { mediaAsset.RawKey };
        if (mediaAsset.FinalKey != null)
        {
            deleteKeys.Add(mediaAsset.FinalKey);
        }

        var tasks = deleteKeys.Select(key => _s3Provider.DeleteFileAsync(key, cancellationToken));
        var deleteResults = await Task.WhenAll(tasks);
        var failedResults = deleteResults.Where(deleteResult => deleteResult.IsFailure).ToList();
        if (failedResults.Any())
        {
            mediaAsset.MarkFailed(DateTime.UtcNow);
            var markFailedSaveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (markFailedSaveResult.IsFailure)
                return markFailedSaveResult.Errors;

            return new Errors(failedResults.SelectMany(deleteResult => deleteResult.Errors).ToArray());
        }

        mediaAsset.MarkDeleted(DateTime.UtcNow);
        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Errors;

        _logger.LogInformation("Deleted file with id:{fileId}", mediaAssetId);

        return new MediaAssetResponse(mediaAssetId);
    }
}