using System.Linq.Expressions;
using System.Text.Json;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.Departments.GetDepartments;
using DirectoryService.Contracts.Positions.GetPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Caching;
using Shared.Result;

namespace DirectoryService.Application.Features.Departments.Queries.GetDepartments
{
    public sealed class GetDepartmentsHandler : IQueryHandler<PaginationResponse<DepartmentDTO>, GetDepartmentsRequest>
    {
        private readonly IReadDbContext _readDbContext;
        private readonly ICacheService _cache;
        private readonly DistributedCacheEntryOptions _cacheOptions;
        private readonly ILogger<GetDepartmentsHandler> _logger;

        public GetDepartmentsHandler(
            IReadDbContext readDbContext,
            ICacheService cache,
            ILogger<GetDepartmentsHandler> logger)
        {
            _readDbContext = readDbContext;
            _cache = cache;
            _cacheOptions = new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(Constants.SLIDING_EXPIRATION_TIME_FROM_MINUTES)
            };
            _logger = logger;
        }

        public async Task<Result<PaginationResponse<DepartmentDTO>>> Handle(GetDepartmentsRequest query, CancellationToken cancellationToken)
        {
            string key = $"{Constants.PREFIX_DEPARTMENT_KEY}{JsonSerializer.Serialize(query)}";

            var cachedDepartments = await _cache.GetOrSetAsync(key, _cacheOptions,
                async () =>
                {
                    var departments = await GetDepartments(query, cancellationToken);

                    return departments;
                },
                cancellationToken);

            if (cachedDepartments is null)
            {
                _logger.LogInformation($"Данные по подразделениям с запросом {query} не найдены в кэше/БД");
            }

            return cachedDepartments!;
        }

        private async Task<PaginationResponse<DepartmentDTO>> GetDepartments(GetDepartmentsRequest request, CancellationToken cancellationToken)
        {
            int totalCount = 0;
            int totalPages = 0;
            List<DepartmentDTO> departments = [];
            var parentIdValue = request.ParentId;
            var locationIdValues = request.LocationIds;

            try
            {
                IQueryable<Department> query = _readDbContext.DepartmentsRead;
                if (locationIdValues != null && locationIdValues.Any())
                {
                    var locationIds = locationIdValues.Select(LocationId.Current).ToList();
                    query = query.Where(d => d.DepartmentLocations.Any(dl => locationIds.Contains(dl.LocationId)));
                }

                if (parentIdValue.HasValue)
                {
                    var parentId = DepartmentId.Current(parentIdValue.Value);
                    query = query.Where(d => d.ParentId == parentId);
                }

                if (string.IsNullOrWhiteSpace(request.Name) == false)
                {
                    query = query.Where(d => d.Name.Value.ToLower().Contains(request.Name.ToLower()));
                }

                if (string.IsNullOrWhiteSpace(request.Identifier) == false)
                {
                    query = query.Where(d => d.Identifier.Value.ToLower().Contains(request.Identifier.ToLower()));
                }

                if (request.IsActive.HasValue)
                {
                    query = query.Where(d => d.IsActive == request.IsActive);
                }

                totalCount = await query.CountAsync(cancellationToken);

                query = query.Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize);

                var orderColumn = request.OrderColumn;
                if (orderColumn != null)
                {
                    Expression<Func<Department, object>> expression = orderColumn.Field switch
                    {
                        OrderField.Name => d => d.Name.Value,
                        OrderField.Path => d => d.Path.Value,
                        _ => d => d.CreatedAt,
                    };
                    query = orderColumn.Direction == OrderDirection.Desc
                        ? query.OrderByDescending(expression)
                        : query.OrderBy(expression);
                }

                totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

                departments = await query.Select(d => new DepartmentDTO
                {
                    Id = d.Id.Value,
                    Name = d.Name.Value,
                    Identifier = d.Identifier.Value,
                    ParentId = d.ParentId == null ? null : d.ParentId.Value,
                    IsActive = d.IsActive,
                    CreatedAt = d.CreatedAt
                }).ToListAsync(cancellationToken);

                _logger.LogInformation("Получение списка подразделений с параметрами {Request}", request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка получения данных о подразделениях с запросом {request}");
            }

            return new PaginationResponse<DepartmentDTO>(departments, totalCount, request.Page, request.PageSize, totalPages);
        }
    }
}
