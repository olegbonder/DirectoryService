using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.Departments.GetChildDepartments;
using DirectoryService.Contracts.Departments.GetRootDepartments;
using Microsoft.Extensions.Logging;
using Shared.Result;

namespace DirectoryService.Application.Features.Departments.Queries.GetDepartments;

public class GetRootDepartmentsHandler : IQueryHandler<GetRootDepartmentsResponse, GetRootDepartmentsRequest>
{
    private readonly IDBConnectionFactory _factory;
    private readonly ILogger<GetRootDepartmentsHandler> _logger;

    public GetRootDepartmentsHandler(
        IDBConnectionFactory factory,
        ILogger<GetRootDepartmentsHandler> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public async Task<Result<GetRootDepartmentsResponse>> Handle(
        GetRootDepartmentsRequest query,
        CancellationToken cancellationToken)
    {
        var dbConnection = await _factory.CreateConnectionAsync(cancellationToken);
        List<RootDepartmentDTO> departments = [];
        int totalCount = 0;
        var pagination = query.Pagination;
        int offset = (pagination.Page - 1) * pagination.PageSize;
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

            await dbConnection.QueryAsync<RootDepartmentDTO, ChildDepartmentDTO?, RootDepartmentDTO>(
                sql,
                param: new
                {
                    root_offset = offset,
                    root_limit = pagination.PageSize,
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

        return new GetRootDepartmentsResponse(departments, totalCount);
    }
}