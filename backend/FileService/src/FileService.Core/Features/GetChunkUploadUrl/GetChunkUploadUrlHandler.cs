using Core.Abstractions;
using Core.Validation;
using FileService.Contracts.Dtos.MediaAssets;
using FileService.Core.FilesStorage;
using FileService.Domain.Assets;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace FileService.Core.Features.GetChunkUploadUrl;

public sealed class GetChunkUploadUrlHandler : ICommandHandler<ChunkUploadUrl, GetChunkUploadUrlCommand>
{
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly ILogger<GetChunkUploadUrlHandler> _logger;
    private readonly IValidator<GetChunkUploadUrlCommand> _validator;
    private readonly IFileStorageProvider _fileStorageProvider;

    public GetChunkUploadUrlHandler(
        IMediaAssetRepository mediaAssetRepository,
        ILogger<GetChunkUploadUrlHandler> logger,
        IValidator<GetChunkUploadUrlCommand> validator,
        IFileStorageProvider fileStorageProvider)
    {
        _mediaAssetRepository = mediaAssetRepository;
        _logger = logger;
        _validator = validator;
        _fileStorageProvider = fileStorageProvider;
    }

    public async Task<Result<ChunkUploadUrl>> Handle(GetChunkUploadUrlCommand command, CancellationToken cancellationToken)
    {
        var validResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validResult.IsValid == false)
        {
            return validResult.ToList();
        }

        var request = command.Request;
        var mediaAssetId = request.MediaAssetId;
        var uploadId = request.UploadId;
        var partNumber = request.PartNumber;

        Result<MediaAsset> mediaAssetResult = await _mediaAssetRepository.GetById(mediaAssetId, cancellationToken);
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Errors;

        MediaAsset mediaAsset = mediaAssetResult.Value;

        Result<string> chunkUploadUrlResult = await _fileStorageProvider.GenerateChunkUploadUrlAsync(mediaAsset.RawKey, uploadId, partNumber, cancellationToken);
        if (chunkUploadUrlResult.IsFailure)
            return chunkUploadUrlResult.Errors;

        _logger.LogInformation("Generated chunk upload url for media asset with id:{mediaAssetId}", mediaAssetId);

        return new ChunkUploadUrl(partNumber, chunkUploadUrlResult.Value);
    }
}
