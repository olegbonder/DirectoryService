using Core.Abstractions;
using Core.Validation;
using FileService.Contracts.Dtos.MediaAssets.GetMediaAssets;
using FileService.Core.Database;
using FileService.Core.FilesStorage;
using FileService.Domain;
using FileService.Domain.Assets;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Result;

namespace FileService.Core.Features.GetMediaAssetsInfo
{
    public sealed class GetMediaAssetsHandler : IQueryHandler<GetMediaAssetsResponse, GetMediaAssetsRequest>
    {
        private readonly IReadDbContext _readDbContext;
        private readonly IValidator<GetMediaAssetsRequest> _validator;
        private readonly IS3Provider _s3Provider;

        public GetMediaAssetsHandler(
            IReadDbContext readDbContext,
            IValidator<GetMediaAssetsRequest> validator,
            IS3Provider s3Provider)
        {
            _readDbContext = readDbContext;
            _validator = validator;
            _s3Provider = s3Provider;
        }

        public async Task<Result<GetMediaAssetsResponse>> Handle(GetMediaAssetsRequest query, CancellationToken cancellationToken)
        {
            var validResult = await _validator.ValidateAsync(query, cancellationToken);
            if (validResult.IsValid == false)
            {
                return validResult.ToList();
            }

            var mediaAssets = await _readDbContext.MediaAssetsQuery
                .Where(m => query.MediaAssetIds.Contains(m.Id) && m.Status != MediaStatus.DELETED).ToListAsync(cancellationToken);

            var readyMediaAssets = mediaAssets.Where(m => m.Status == MediaStatus.READY).ToList();
            var keys = readyMediaAssets.Select(m => m.RawKey).ToList();
            var urlsResult = await _s3Provider.GenerateDownloadUrlsAsync(keys, cancellationToken);
            if (urlsResult.IsFailure)
                return urlsResult.Errors;

            var urls = urlsResult.Value;
            var urlsDict = urls.ToDictionary(u => u.StorageKey, u => u.PresignedUrl);

            var results = new List<GetMediaAssetsDto>();
            foreach (MediaAsset mediaAsset in mediaAssets)
            {
                string? downloadUrl = null;
                if (urlsDict.TryGetValue(mediaAsset.RawKey, out string? url))
                {
                    downloadUrl = url;
                }

                var mediaAssetDto = new GetMediaAssetsDto(
                    mediaAsset.Id,
                    mediaAsset.Status.ToString().ToLowerInvariant(),
                    downloadUrl);
                results.Add(mediaAssetDto);
            }

            return new GetMediaAssetsResponse(results);
        }
    }
}