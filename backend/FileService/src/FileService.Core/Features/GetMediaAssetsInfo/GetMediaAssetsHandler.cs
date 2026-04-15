using Core.Abstractions;
using Core.Validation;
using FileService.Contracts.Dtos.MediaAssets.GetMediaAssets;
using FileService.Core.Database;
using FileService.Core.FilesStorage;
using FileService.Domain;
using FileService.Domain.Assets;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using SharedKernel.Result;

namespace FileService.Core.Features.GetMediaAssetsInfo
{
    public sealed class GetMediaAssetsHandler : IQueryHandler<GetMediaAssetsResponse, GetMediaAssetsRequest>
    {
        private readonly IReadDbContext _readDbContext;
        private readonly IValidator<GetMediaAssetsRequest> _validator;
        private readonly IFileStorageProvider _fileStorageProvider;
        private readonly HybridCache _cache;
        private readonly FileStorageOptions _fileStorageOptions;
        private readonly SemaphoreSlim _requestsSemaphore;

        public GetMediaAssetsHandler(
            IReadDbContext readDbContext,
            IValidator<GetMediaAssetsRequest> validator,
            IFileStorageProvider fileStorageProvider,
            HybridCache cache,
            IOptions<FileStorageOptions> fileStorageOptions)
        {
            _readDbContext = readDbContext;
            _validator = validator;
            _fileStorageProvider = fileStorageProvider;
            _cache = cache;
            _fileStorageOptions = fileStorageOptions.Value;
            _requestsSemaphore = new SemaphoreSlim(1, _fileStorageOptions.MaxConcurrentRequests);
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

            var urls = await GetPresignedUrlsFromCacheAsync(keys, cancellationToken);

            var results = new List<GetMediaAssetsDto>();
            foreach (MediaAsset mediaAsset in mediaAssets)
            {
                string? downloadUrl = null;
                if (urls.TryGetValue(mediaAsset.RawKey, out string? url))
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

        private async Task<Dictionary<StorageKey, string>> GetPresignedUrlsFromCacheAsync(
            IEnumerable<StorageKey> storageKeys,
            CancellationToken cancellationToken)
        {
            var keys = storageKeys.ToList();

            if (!keys.Any())
                return [];

            var cachedUrlTasks = keys.Select(async key =>
            {
                await _requestsSemaphore.WaitAsync(cancellationToken);
                try
                {
                    string? url = await _cache.GetOrCreateAsync<string?>(
                        key: key.Value,
                        factory: _ => ValueTask.FromResult<string?>(null),
                        options: new HybridCacheEntryOptions
                        {
                            Expiration = TimeSpan.FromHours(_fileStorageOptions.DownloadExpirationHours)
                                .Subtract(TimeSpan.FromHours(1)),
                            LocalCacheExpiration = TimeSpan.FromHours(1)
                        },
                        cancellationToken: cancellationToken);
                    return (key, url);
                }
                finally
                {
                    _requestsSemaphore.Release();
                }
            });

            var cachedUrls = await Task.WhenAll(cachedUrlTasks);

            var result = new Dictionary<StorageKey, string>();

            var keysToGenerate = new List<StorageKey>();

            foreach ((StorageKey key, string? url) in cachedUrls)
            {
                if (!string.IsNullOrWhiteSpace(url))
                {
                    result[key] = url;
                }
                else
                {
                    keysToGenerate.Add(key);
                }
            }

            if (keysToGenerate.Any())
            {
                var urlsResult = await _fileStorageProvider
                    .GenerateDownloadUrlsAsync(keysToGenerate, cancellationToken);
                if (urlsResult.IsFailure)
                    return result;

                var mediaUrls = urlsResult.Value;

                var setTasks = mediaUrls.Select(async mediaUrl =>
                {
                    result[mediaUrl.StorageKey] = mediaUrl.PresignedUrl;

                    await _cache.SetAsync(
                        key: mediaUrl.StorageKey.Value,
                        value: mediaUrl.PresignedUrl,
                        new HybridCacheEntryOptions
                        {
                            Expiration = TimeSpan.FromHours(_fileStorageOptions.DownloadExpirationHours)
                                .Subtract(TimeSpan.FromHours(1))
                        },
                        cancellationToken: cancellationToken);
                });

                await Task.WhenAll(setTasks);
            }

            return result;
        }
    }
}