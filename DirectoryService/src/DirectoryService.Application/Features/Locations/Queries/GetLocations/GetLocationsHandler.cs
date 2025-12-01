using System.Text.Json;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Result;

namespace DirectoryService.Application.Features.Locations.Queries.GetLocations
{
    public sealed class GetLocationsHandler : IQueryHandler<GetLocationsResponse, GetLocationsRequest>
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

        public async Task<Result<GetLocationsResponse>> Handle(GetLocationsRequest request, CancellationToken cancellationToken)
        {
            int totalCount = 0;
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

                    var pagination = request.Pagination;
                    query = query.OrderBy(l => l.Name.Value).ThenBy(l => l.CreatedAt);

                    totalCount = await query.CountAsync(cancellationToken);

                    query = query.Skip((pagination.Page - 1) * pagination.PageSize)
                        .Take(pagination.PageSize);

                }

                var locations = await query.Select(l => new LocationDTO
                {
                    Id = l.Id.Value,
                    Name = l.Name.Value,
                    Country = l.Address.Country,
                    City = l.Address.City,
                    Street = l.Address.Street,
                    HouseNumber = l.Address.HouseNumber,
                    FlatNumber = l.Address.FlatNumber,
                    Timezone = l.Timezone.Value,
                    IsActive = l.IsActive,
                    CreatedAt = l.CreatedAt
                }).ToListAsync(cancellationToken);

                _logger.LogInformation("Получение списка локаций");

                return new GetLocationsResponse(locations, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка получения данных о локациях с запросом {request}");
                return new GetLocationsResponse([], totalCount);
            }
        }
    }
}
