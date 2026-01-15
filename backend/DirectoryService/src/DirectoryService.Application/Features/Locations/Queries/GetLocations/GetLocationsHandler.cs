using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.Locations.GetLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Result;

namespace DirectoryService.Application.Features.Locations.Queries.GetLocations
{
    public sealed class GetLocationsHandler : IQueryHandler<PaginationResponse<LocationDTO>, GetLocationsRequest>
    {
        private readonly IReadDbContext _readDbContext;
        private readonly ILogger<GetLocationsHandler> _logger;

        public GetLocationsHandler(
            IReadDbContext readDbContext,
            ILogger<GetLocationsHandler> logger)
        {
            _readDbContext = readDbContext;
            _logger = logger;
        }

        public async Task<Result<PaginationResponse<LocationDTO>>> Handle(GetLocationsRequest request, CancellationToken cancellationToken)
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
                            .SelectMany(d => d.DepartmentLocations.Select(dl => dl.Location))
                            .OrderBy(l => l.Name.Value).ThenBy(l => l.CreatedAt);

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

                    if (request.Order.HasValue)
                    {
                        if (request.Order.HasValue)
                        {
                            query = request.Order.Value == OrderBy.Asc
                                ? query.OrderBy(l => l.CreatedAt)
                                : query.OrderByDescending(l => l.CreatedAt);
                        }
                    }
                    else
                    {
                        query = query.OrderBy(l => l.Name.Value).ThenBy(l => l.CreatedAt);
                    }

                    totalCount = await query.CountAsync(cancellationToken);

                    query = query.Skip((request.Page - 1) * request.PageSize)
                        .Take(request.PageSize);

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
