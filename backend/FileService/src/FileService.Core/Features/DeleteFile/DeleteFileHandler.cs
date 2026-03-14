using Core.Abstractions;
using Core.Validation;
using FileService.Core.FilesStorage;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace FileService.Core.Features.DeleteFile;

public class DeleteFileHandler : ICommandHandler<Guid, DeleteFileCommand>
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

    public async Task<Result<Guid>> Handle(DeleteFileCommand command, CancellationToken cancellationToken)
    {
        var validResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validResult.IsValid == false)
        {
            return validResult.ToList();
        }

        var fileId = command.FileId;
        var mediaAssetResult = await _mediaAssetRepository.GetById(fileId, cancellationToken);
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Errors;

        var mediaAsset = mediaAssetResult.Value;

        var deleteResult = await _s3Provider.DeleteFileAsync(
            mediaAsset.RawKey,
            cancellationToken);
        if (deleteResult.IsFailure)
        {
            mediaAsset.MarkFailed(DateTime.UtcNow);
            await _mediaAssetRepository.SaveChanges(cancellationToken);
            return deleteResult.Errors;
        }

        mediaAsset.MarkDeleted(DateTime.UtcNow);
        await _mediaAssetRepository.SaveChanges(cancellationToken);

        _logger.LogInformation("Deleted file with id:{fileId}", fileId);

        return fileId;
    }
}