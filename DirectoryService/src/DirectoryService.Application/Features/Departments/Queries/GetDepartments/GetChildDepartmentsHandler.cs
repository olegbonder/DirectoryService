using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.Departments.GetChildDepartments;
using Microsoft.Extensions.Logging;
using Shared.Result;

namespace DirectoryService.Application.Features.Departments.Queries.GetDepartments;

public class GetChildDepartmentsHandler : IQueryHandler<GetChildDepartmentsResponse, GetChildDepartmentsQuery>
{
    private readonly IDBConnectionFactory _factory;
    private readonly ILogger<GetRootDepartmentsHandler> _logger;

    public GetChildDepartmentsHandler(
        IDBConnectionFactory factory,
        ILogger<GetRootDepartmentsHandler> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public async Task<Result<GetChildDepartmentsResponse>> Handle(
        GetChildDepartmentsQuery query,
        CancellationToken cancellationToken)
    {
        List<ChildDepartmentDTO> departments = [];
        int totalCount = 0;
        Guid parentId = query.ParentId;
        if (parentId == Guid.Empty)
        {
            return new GetChildDepartmentsResponse(departments, totalCount);
        }

        var pagination = query.Request.Pagination;
        int offset = (pagination.Page - 1) * pagination.PageSize;
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
            departments = (await dbConnection.QueryAsync<ChildDepartmentDTO>(
                sql,
                param: new
                {
                    offset = offset,
                    limit = pagination.PageSize,
                    parentId
                })).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка получения данных о подразделениях с запросом {query}");
        }

        return new GetChildDepartmentsResponse(departments, totalCount);
    }
}