using System.Text.Json;
using Core.Abstractions;
using Core.Caching;
using Core.Database;
using Dapper;
using DirectoryService.Contracts.Departments.GetTopDepartments;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace DirectoryService.Application.Features.Departments.Queries.GetTopDepartments;

public sealed class GetTopDepartmentsHandler: IQueryHandler<GetTopDepartmentsResponse, GetTopDepartmentsRequest>
{
    private readonly IDbConnectionFactory _factory;
    private readonly ICacheService _cache;
    private readonly ILogger<GetTopDepartmentsHandler> _logger;
    private readonly DistributedCacheEntryOptions _cacheOptions;

    public GetTopDepartmentsHandler(
        IDbConnectionFactory factory,
        ICacheService cache,
        ILogger<GetTopDepartmentsHandler> logger)
    {
        _factory = factory;
        _cache = cache;
        _cacheOptions = new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(Constants.SLIDING_EXPIRATION_TIME_FROM_MINUTES)
        };
        _logger = logger;
    }

    public async Task<Result<GetTopDepartmentsResponse>> Handle(
        GetTopDepartmentsRequest query,
        CancellationToken cancellationToken)
    {
        string key = $"{Constants.PREFIX_DEPARTMENT_KEY}{JsonSerializer.Serialize(query)}";

        var cachedDepartments = await _cache.GetOrSetAsync(key, _cacheOptions,
            async () =>
            {
                var departments = await GetTopDepartments(query, cancellationToken);

                return departments;
            },
            cancellationToken);

        if (cachedDepartments is null)
        {
            _logger.LogInformation($"Данные по топ {query.LimitTop} подразделений с позициями с запросом {query} не найдены в кэше/БД");
        }

        return cachedDepartments!;
    }

    private async Task<GetTopDepartmentsResponse> GetTopDepartments(
        GetTopDepartmentsRequest query,
        CancellationToken cancellationToken)
    {
        var dbConnection = await _factory.CreateConnectionAsync(cancellationToken);
        List<TopDepartmentDTO> departments = [];
        int totalCount = 0;
        string sql =
            """
            SELECT d.id, d.name,
                   d.is_active, d.path, d.depth, d.created_at,  
                   COUNT(dp.*) positionsCount
            FROM departments d
            LEFT JOIN public.department_positions dp on d.id = dp.department_id
            GROUP BY d.id
            ORDER BY positionsCount DESC 
            LIMIT @limitTop
            """;
        try
        {
            totalCount = await dbConnection.ExecuteScalarAsync<int>(@"SELECT COUNT(*) FROM departments", cancellationToken);
            departments = (await dbConnection.QueryAsync<TopDepartmentDTO>(
                sql,
                param: new { limitTop = query.LimitTop })).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка получения данных о подразделениях с запросом {query}");
        }

        return new GetTopDepartmentsResponse(departments, totalCount);
    }
}