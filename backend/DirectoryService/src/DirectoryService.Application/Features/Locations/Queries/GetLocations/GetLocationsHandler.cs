using System.Text.Json;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.Locations.GetLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Caching;
using Shared.Result;

namespace DirectoryService.Application.Features.Locations.Queries.GetLocations
{
    public sealed class GetLocationsHandler : IQueryHandler<PaginationResponse<LocationDTO>, GetLocationsRequest>
    {
        private readonly IReadDbContext _readDbContext;
        private readonly ICacheService _cache;
        private readonly DistributedCacheEntryOptions _cacheOptions;
        private readonly ILogger<GetLocationsHandler> _logger;

        public GetLocationsHandler(
            IReadDbContext readDbContext,
            ICacheService cache,
            ILogger<GetLocationsHandler> logger)
        {
            _readDbContext = readDbContext;
            _cache = cache;
            _cacheOptions = new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(Constants.SLIDING_EXPIRATION_TIME_FROM_MINUTES)
            };
            _logger = logger;
        }

        public async Task<Result<PaginationResponse<LocationDTO>>> Handle(GetLocationsRequest query, CancellationToken cancellationToken)
        {
            string key = $"{Constants.PREFIX_LOCATION_KEY}{JsonSerializer.Serialize(query)}";

            var cachedLocations = await _cache.GetOrSetAsync(key, _cacheOptions,
                async () =>
                {
                    var locations = await GetLocations(query, cancellationToken);

                    return locations;
                },
                cancellationToken);

            if (cachedLocations is null)
            {
                _logger.LogInformation($"Данные по локациям с запросом {query} не найдены в кэше/БД");
            }

            return cachedLocations!;
        }

        private async Task<PaginationResponse<LocationDTO>> GetLocations(GetLocationsRequest request, CancellationToken cancellationToken)
        {
            int totalCount = 0;
            int totalPages = 0;
            List<LocationDTO> locations = [];
            var departmentIdValues = request.DepartmentIds;

            try
            {
                IQueryable<Location> query;
                if (departmentIdValues != null && departmentIdValues.Any())
                {
                    var departmentIds = departmentIdValues.Select(DepartmentId.Current).ToList();
                    query = _readDbContext.DepartmentsRead
                            .Where(d => departmentIds.Contains(d.Id))
                            .SelectMany(d => d.DepartmentLocations.Select(dl => dl.Location));

                    totalCount = await query.CountAsync(cancellationToken);
                }
                else
                {
                    query = _readDbContext.LocationsRead;
                    if (string.IsNullOrWhiteSpace(request.Search) == false)
                    {
                        query = query.Where(l => l.Name.Value.ToLower().Contains(request.Search.ToLower()));
                    }

                    if (request.IsActive.HasValue)
                    {
                        query = query.Where(l => l.IsActive == request.IsActive);
                    }

                    totalCount = await query.CountAsync(cancellationToken);

                    query = query.Skip((request.Page - 1) * request.PageSize)
                        .Take(request.PageSize);

                }

                if (request.Order.HasValue)
                {
                    if (request.Order.HasValue)
                    {
                        query = request.Order.Value == OrderDirection.Asc
                            ? query.OrderBy(l => l.CreatedAt)
                            : query.OrderByDescending(l => l.CreatedAt);
                    }
                }
                else
                {
                    query = query.OrderBy(l => l.Name.Value).ThenBy(l => l.CreatedAt);
                }

                locations = await query.Select(l => new LocationDTO
                {
                    Id = l.Id.Value,
                    Name = l.Name.Value,
                    Country = l.Address.Country,
                    City = l.Address.City,
                    Street = l.Address.Street,
                    House = l.Address.HouseNumber,
                    Flat = l.Address.FlatNumber,
                    TimeZone = l.Timezone.Value,
                    IsActive = l.IsActive,
                    CreatedAt = l.CreatedAt
                }).ToListAsync(cancellationToken);

                totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
                _logger.LogInformation("Получение списка локаций");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка получения данных о локациях с запросом {request}");
            }

            return new PaginationResponse<LocationDTO>(locations, totalCount, request.Page, request.PageSize, totalPages);
        }
    }
}
