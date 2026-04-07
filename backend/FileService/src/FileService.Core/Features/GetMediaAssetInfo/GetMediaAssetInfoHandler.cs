using Core.Abstractions;
using Core.Validation;
using FileService.Contracts.Dtos.MediaAssets.GetMediaAsset;
using FileService.Core.Database;
using FileService.Core.FilesStorage;
using FileService.Domain;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Result;

namespace FileService.Core.Features.GetMediaAssetInfo
{
    public sealed class GetMediaAssetInfoHandler : IQueryHandler<GetMediaAssetResponse?, GetMediaAssetInfoRequest>
    {
        private readonly IReadDbContext _readDbContext;
        private readonly IValidator<GetMediaAssetInfoRequest> _validator;
        private readonly IS3Provider _s3Provider;

        public GetMediaAssetInfoHandler(
            IReadDbContext readDbContext,
            IValidator<GetMediaAssetInfoRequest> validator,
            IS3Provider s3Provider)
        {
            _readDbContext = readDbContext;
            _validator = validator;
            _s3Provider = s3Provider;
        }

        public async Task<Result<GetMediaAssetResponse?>> Handle(GetMediaAssetInfoRequest request, CancellationToken cancellationToken)
        {
            var validResult = await _validator.ValidateAsync(request, cancellationToken);
            if (validResult.IsValid == false)
            {
                return validResult.ToList();
            }

            var mediaAssetId = request.MediaAssetId;
            var mediaAsset = await _readDbContext.MediaAssetsQuery
                .FirstOrDefaultAsync(m => m.Id == mediaAssetId, cancellationToken);

            if (mediaAsset == null)
                return Result<GetMediaAssetResponse?>.Success(null);

            string? url = null;

            if (mediaAsset.Status == MediaStatus.READY)
            {
                var urlsResult = await _s3Provider.GenerateDownloadUrlAsync(mediaAsset.RawKey);
                if (urlsResult.IsFailure)
                    return urlsResult.Errors;

                url = urlsResult.Value;
            }

            var fileInfo = new FileInfoDto(mediaAsset.MediaData.FileName.Value, mediaAsset.MediaData.ContentType.Value, mediaAsset.MediaData.Size);
            var mediaAssetDto = new GetMediaAssetResponse(
                    mediaAsset.Id,
                    mediaAsset.Status.ToString().ToLowerInvariant(),
                    mediaAsset.AssetType.ToString().ToLowerInvariant(),
                    mediaAsset.CreatedAt,
                    mediaAsset.UpdatedAt,
                    fileInfo,
                    url);

            return mediaAssetDto;
        }
    }
}