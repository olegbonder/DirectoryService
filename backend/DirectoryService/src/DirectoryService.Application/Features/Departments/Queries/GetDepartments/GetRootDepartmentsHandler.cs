using System.Text.Json;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.Departments.GetChildDepartments;
using DirectoryService.Contracts.Departments.GetRootDepartments;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Caching;
using Shared.Result;

namespace DirectoryService.Application.Features.Departments.Queries.GetDepartments;

public class GetRootDepartmentsHandler : IQueryHandler<PaginationResponse<RootDepartmentDTO>, GetRootDepartmentsRequest>
{
    private readonly IDBConnectionFactory _factory;
    private readonly ICacheService _cache;
    private readonly ILogger<GetRootDepartmentsHandler> _logger;
    private readonly DistributedCacheEntryOptions _cacheOptions;

    public GetRootDepartmentsHandler(
        IDBConnectionFactory factory,
        ICacheService cache,
        ILogger<GetRootDepartmentsHandler> logger)
    {
        _factory = factory;
        _cache = cache;
        _cacheOptions = new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(Constants.SLIDING_EXPIRATION_TIME_FROM_MINUTES)
        };
        _logger = logger;
    }

    public async Task<Result<PaginationResponse<RootDepartmentDTO>>> Handle(
        GetRootDepartmentsRequest query,
        CancellationToken cancellationToken)
    {
        string key = $"{Constants.PREFIX_DEPARTMENT_KEY}{JsonSerializer.Serialize(query)}";

        var cachedDepartments = await _cache.GetOrSetAsync(key, _cacheOptions,
            async () =>
            {
                var departments = await GetRootDepartments(query, cancellationToken);

                return departments;
            },
            cancellationToken);

        if (cachedDepartments is null)
        {
            _logger.LogInformation($"Данные по корневым подразделениям с запросом {query} не найдены в кэше/БД");
        }

        return cachedDepartments!;
    }

    private async Task<PaginationResponse<RootDepartmentDTO>> GetRootDepartments(
        GetRootDepartmentsRequest query,
        CancellationToken cancellationToken)
    {
        var dbConnection = await _factory.CreateConnectionAsync(cancellationToken);
        List<RootDepartmentDTO> departments = [];
        int totalCount = 0;
        int totalPages = 0;
        int offset = (query.Page - 1) * query.PageSize;
        string sql =
            """
            WITH roots AS (SELECT d.id,
                                  d.parent_id,
                                  d.name,
                                  d.is_active IsActive,
                                  d.identifier,
                                  d.depth,
                                  d.path,
                                  d.created_at,
                                  EXISTS(SELECT 1 FROM departments WHERE parent_id = d.id OFFSET @child_limit LIMIT 1) hasMoreChildren
                           FROM departments d
                           WHERE d.parent_id is null
                           ORDER BY d.created_at               
                           OFFSET @root_offset LIMIT @root_limit)
            
            SELECT r.*, 
                   null id, null parent_id, null name, null is_active, 
                   null identifier, null depth, null path, null created_at,
                   null hasMoreChildren      
            FROM roots r
            
            UNION ALL
            
            SELECT r.*, c.*,
                   EXISTS(SELECT 1 FROM departments d WHERE d.parent_id = c.id) hasMoreChildren
            FROM roots r 
            CROSS JOIN LATERAL 
                (SELECT d.id,
                        d.parent_id,
                        d.name,
                        d.is_active,
                        d.identifier,
                        d.depth,
                        d.path,
                        d.created_at
                 FROM departments d
                 WHERE d.parent_id = r.id 
                 AND d.is_active = true
                 ORDER BY d.created_at
                 LIMIT @child_limit) c
            """;
        try
        {
            totalCount =
                await dbConnection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM departments WHERE parent_id is null", cancellationToken);
            totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

            await dbConnection.QueryAsync<RootDepartmentDTO, ChildDepartmentDTO?, RootDepartmentDTO>(
                sql,
                param: new
                {
                    root_offset = offset,
                    root_limit = query.PageSize,
                    child_limit = query.Prefetch
                },
                map: (parent, child) =>
                {
                    var existParent = departments.FirstOrDefault(d => d.Id == parent.Id);
                    if (existParent == null)
                    {
                        departments.Add(parent);
                        if (child != null)
                        {
                            parent.Children.Add(child);
                        }

                        return parent;
                    }
                    else
                    {
                        if (child != null)
                        {
                            existParent.Children.Add(child);
                        }

                        return existParent;
                    }
                },
                splitOn: "id");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка получения данных о подразделениях с запросом {query}");
        }

        return new PaginationResponse<RootDepartmentDTO>(departments, totalCount, query.Page, query.PageSize, totalPages);
    }
}