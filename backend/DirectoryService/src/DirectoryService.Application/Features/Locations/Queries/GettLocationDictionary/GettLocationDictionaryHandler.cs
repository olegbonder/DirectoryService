using System.Text.Json;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Locations.GetLocationDictionary;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Caching;
using Shared.Result;

namespace DirectoryService.Application.Features.Locations.Queries.GettLocationDictionary
{
    public sealed class GettLocationDictionaryHandler : IQueryHandler<PaginationResponse<DictionaryItemResponse>, GetLocationDictionaryRequest>
    {
        private readonly IReadDbContext _readDbContext;
        private readonly ICacheService _cache;
        private readonly DistributedCacheEntryOptions _cacheOptions;
        private readonly ILogger<GettLocationDictionaryHandler> _logger;

        public GettLocationDictionaryHandler(
            IReadDbContext readDbContext,
            ICacheService cache,
            ILogger<GettLocationDictionaryHandler> logger)
        {
            _readDbContext = readDbContext;
            _cache = cache;
            _cacheOptions = new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(Constants.SLIDING_EXPIRATION_TIME_FROM_MINUTES)
            };
            _logger = logger;
        }

        public async Task<Result<PaginationResponse<DictionaryItemResponse>>> Handle(GetLocationDictionaryRequest query, CancellationToken cancellationToken)
        {
            string key = $"{Constants.PREFIX_LOCATION_KEY}{JsonSerializer.Serialize(query)}";

            var cachedLocationDictionary = await _cache.GetOrSetAsync(key, _cacheOptions,
                async () =>
                {
                    var locationDictionary = await GetLocationDictionary(query, cancellationToken);

                    return locationDictionary;
                },
                cancellationToken);

            if (cachedLocationDictionary is null)
            {
                _logger.LogInformation($"Данные по подразделениям не найдены в кэше/БД");
            }

            return cachedLocationDictionary!;
        }

        private async Task<PaginationResponse<DictionaryItemResponse>> GetLocationDictionary(GetLocationDictionaryRequest request, CancellationToken cancellationToken)
        {
            int totalCount = 0;
            int totalPages = 0;
            List<DictionaryItemResponse> items = [];
            var locationIdValues = request.LocationIds;
            try
            {
                IQueryable<Location> query = _readDbContext.LocationsRead.Where(l => l.IsActive);
                if (locationIdValues != null && locationIdValues.Any())
                {
                    var locationIds = locationIdValues.Select(LocationId.Current).ToList();
                    query = query.Where(l => locationIds.Contains(l.Id));
                }

                if (string.IsNullOrWhiteSpace(request.Search) == false)
                {
                    query = query.Where(l => l.Name.Value.ToLower().Contains(request.Search.ToLower()));
                }

                totalCount = await query.CountAsync(cancellationToken);

                query = query.Skip((request.Page - 1) * request.PageSize)
                        .Take(request.PageSize);

                items = await query
                    .OrderBy(l => l.Name.Value)
                    .Select(l => new DictionaryItemResponse(l.Id.Value, l.Name.Value))
                    .ToListAsync(cancellationToken);

                totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

                _logger.LogInformation("Получение справочника локаций");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения справочника локаций с запросом {request}", JsonSerializer.Serialize(request));
            }

            return new PaginationResponse<DictionaryItemResponse>(items, totalCount, request.Page, request.PageSize, totalPages);
        }
    }
}
