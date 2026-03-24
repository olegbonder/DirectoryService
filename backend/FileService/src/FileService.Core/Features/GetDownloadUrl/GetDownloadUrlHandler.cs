using Core.Abstractions;
using Core.Validation;
using FileService.Contracts.Dtos.MediaAssets.GetDownloadUrl;
using FileService.Core.Features.GetChunkUploadUrl;
using FileService.Core.FilesStorage;
using FileService.Domain.Assets;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace FileService.Core.Features.GetDownloadUrl;

public sealed class GetDownloadUrlHandler : ICommandHandler<GetDownloadUrlResponse, GetDownloadUrlCommand>
{
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly ILogger<GetChunkUploadUrlHandler> _logger;
    private readonly IValidator<GetDownloadUrlCommand> _validator;
    private readonly IS3Provider _s3Provider;

    public GetDownloadUrlHandler(
        IMediaAssetRepository mediaAssetRepository,
        ILogger<GetChunkUploadUrlHandler> logger,
        IValidator<GetDownloadUrlCommand> validator,
        IS3Provider s3Provider)
    {
        _mediaAssetRepository = mediaAssetRepository;
        _logger = logger;
        _validator = validator;
        _s3Provider = s3Provider;
    }

    public async Task<Result<GetDownloadUrlResponse>> Handle(GetDownloadUrlCommand command, CancellationToken cancellationToken)
    {
        var validResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validResult.IsValid == false)
        {
            return validResult.ToList();
        }

        var request = command.Request;
        var mediaAssetId = request.MediaAssetId;

        Result<MediaAsset> mediaAssetResult = await _mediaAssetRepository.GetById(mediaAssetId, cancellationToken);
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Errors;

        MediaAsset mediaAsset = mediaAssetResult.Value;

        Result<string> getDownloadUrlResult = await _s3Provider.GenerateDownloadUrlAsync(mediaAsset.RawKey);
        if (getDownloadUrlResult.IsFailure)
            return getDownloadUrlResult.Errors;

        _logger.LogInformation("Generated download url for media asset with id:{mediaAssetId}", mediaAssetId);

        return new GetDownloadUrlResponse(getDownloadUrlResult.Value);
    }
}
