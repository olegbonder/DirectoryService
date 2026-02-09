using System.Text.Json;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.Departments.GetChildDepartments;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Caching;
using Shared.Result;

namespace DirectoryService.Application.Features.Departments.Queries.GetDepartments;

public class GetChildDepartmentsHandler : IQueryHandler<PaginationResponse<ChildDepartmentDTO>, GetChildDepartmentsQuery>
{
    private readonly IDBConnectionFactory _factory;
    private readonly ILogger<GetRootDepartmentsHandler> _logger;
    private readonly ICacheService _cache;
    private readonly DistributedCacheEntryOptions _cacheOptions;

    public GetChildDepartmentsHandler(
        IDBConnectionFactory factory,
        ICacheService cache,
        ILogger<GetRootDepartmentsHandler> logger)
    {
        _factory = factory;
        _logger = logger;
        _cache = cache;
        _cacheOptions = new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(Constants.SLIDING_EXPIRATION_TIME_FROM_MINUTES)
        };
    }

    public async Task<Result<PaginationResponse<ChildDepartmentDTO>>> Handle(
        GetChildDepartmentsQuery query,
        CancellationToken cancellationToken)
    {
        string key = $"{Constants.PREFIX_DEPARTMENT_KEY}{JsonSerializer.Serialize(query)}";

        var cachedDepartments = await _cache.GetOrSetAsync(key, _cacheOptions,
            async () =>
            {
                var departments = await GetChildDepartments(query, cancellationToken);

                return departments;
            },
            cancellationToken);

        if (cachedDepartments is null)
        {
            _logger.LogInformation($"Данные по дочерним подразделениям с запросом {query} не найдены в кэше/БД");
        }

        return cachedDepartments!;
    }

    private async Task<PaginationResponse<ChildDepartmentDTO>> GetChildDepartments(
        GetChildDepartmentsQuery query,
        CancellationToken cancellationToken)
    {
        List<ChildDepartmentDTO> departments = [];
        int totalCount = 0;
        int totalPages = 0;
        Guid parentId = query.ParentId;
        var request = query.Request;
        if (parentId == Guid.Empty)
        {
            return new PaginationResponse<ChildDepartmentDTO>(departments, totalCount, request.Page, request.PageSize, totalPages);
        }

        int offset = (request.Page - 1) * request.PageSize;
        string sql =
            """
            SELECT d.id, d.parent_id, d.name,
                d.is_active IsActive ,d.identifier, d.depth, 
                d.path, d.created_at,
                EXISTS(SELECT 1 FROM departments WHERE parent_id = d.id) hasMoreChildren
            FROM departments d
            WHERE d.parent_id = @parentId
            ORDER BY d.created_at
            OFFSET @offset LIMIT @limit
            """;
        try
        {
            var dbConnection = await _factory.CreateConnectionAsync(cancellationToken);
            totalCount = await dbConnection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM departments WHERE parent_id = @parentId",
                new { parentId });
            totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
            departments = (await dbConnection.QueryAsync<ChildDepartmentDTO>(
                sql,
                param: new
                {
                    offset,
                    limit = request.PageSize,
                    parentId
                })).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка получения данных о подразделениях с запросом {query}");
        }

        return new PaginationResponse<ChildDepartmentDTO>(departments, totalCount, request.Page, request.PageSize, totalPages);
    }
}