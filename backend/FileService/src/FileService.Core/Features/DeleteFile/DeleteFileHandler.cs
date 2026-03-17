using Core.Abstractions;
using Core.Validation;
using FileService.Contracts.MediaAssets;
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

    public DeleteFileHandler(
        IMediaAssetRepository mediaAssetRepository,
        IS3Provider s3Provider,
        IValidator<DeleteFileCommand> validator,
        ILogger<DeleteFileHandler> logger)
    {
        _mediaAssetRepository = mediaAssetRepository;
        _s3Provider = s3Provider;
        _validator = validator;
        _logger = logger;
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
        var failedResults = deleteResults.Where(deleteResult => deleteResult.IsFailure);
        if (failedResults.Any())
        {
            mediaAsset.MarkFailed(DateTime.UtcNow);
            await _mediaAssetRepository.SaveChanges(cancellationToken);
            return new Errors(failedResults.SelectMany(deleteResult => deleteResult.Errors));
        }

        mediaAsset.MarkDeleted(DateTime.UtcNow);
        await _mediaAssetRepository.SaveChanges(cancellationToken);

        _logger.LogInformation("Deleted file with id:{fileId}", mediaAssetId);

        return new MediaAssetResponse(mediaAssetId);
    }
}