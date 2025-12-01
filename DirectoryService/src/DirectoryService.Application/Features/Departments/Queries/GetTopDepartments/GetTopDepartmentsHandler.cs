using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.Departments;
using Microsoft.Extensions.Logging;
using Shared.Result;

namespace DirectoryService.Application.Features.Departments.Queries.GetTopDepartments;

public sealed class GetTopDepartmentsHandler: IQueryHandler<GetTopDepartmentsResponse, GetTopDepartmentsRequest>
{
    private readonly IDBConnectionFactory _factory;
    private readonly ILogger<GetTopDepartmentsHandler> _logger;

    public GetTopDepartmentsHandler(
        IDBConnectionFactory factory,
        ILogger<GetTopDepartmentsHandler> logger)
    {
        _factory = factory;
        _logger = logger;
    }


    public async Task<Result<GetTopDepartmentsResponse>> Handle(
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