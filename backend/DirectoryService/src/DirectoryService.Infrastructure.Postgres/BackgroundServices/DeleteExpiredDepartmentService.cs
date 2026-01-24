using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using DirectoryService.Infrastructure.Postgres.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Caching;
using Shared.Result;

namespace DirectoryService.Infrastructure.Postgres.BackgroundServices;

public class DeleteExpiredDepartmentService
{
    private readonly ILogger<DeleteExpiredDepartmentService> _logger;
    private readonly TransactionManager _transactionManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly ICacheService _cache;

    public DeleteExpiredDepartmentService(
        ILogger<DeleteExpiredDepartmentService> logger,
        TransactionManager transactionManager,
        ApplicationDbContext dbContext,
        ICacheService cache)
    {
        _logger = logger;
        _transactionManager = transactionManager;
        _dbContext = dbContext;
        _cache = cache;
    }

    public async Task Process(CancellationToken cancellationToken)
    {
        string prefixDepartmentKey = "departments_";
        var transactionScopeResult = await _transactionManager.BeginTransaction(cancellationToken: cancellationToken);
        if (transactionScopeResult.IsFailure)
        {
            return;
        }

        using var transactionScope = transactionScopeResult.Value;

        var departments = await GetExpiredDepartments(cancellationToken);
        if (departments.Any())
        {
            var departmentPaths = departments.Select(d => d.Path.Value).ToArray();
            var lockDepartments = await GetDepartmentsWithLock(departmentPaths, cancellationToken);
            if (lockDepartments.Count < departments.Count)
            {
                transactionScope.RollBack();
                _logger.LogError("Отмена операции удаления подразделений из-за несовпадения количества заблокированных записей");
                return;
            }

            var updChildrenResult = await UpdateChildrenDepartments(departmentPaths, cancellationToken);
            if (updChildrenResult.IsFailure)
            {
                transactionScope.RollBack();
                var errors = updChildrenResult.Errors.Select(e => e.Message);
                _logger.LogError(string.Join(", ", errors));
                return;
            }

            var deleteResult = await DeleteDepartments(departmentPaths, cancellationToken);
            if (deleteResult.IsFailure)
            {
                transactionScope.RollBack();
                var errors = deleteResult.Errors.Select(e => e.Message);
                _logger.LogError(string.Join(", ", errors));
                return;
            }

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                transactionScope.RollBack();
                _logger.LogError(ex, "Отмена операции удаления подразделений/позиций/локаций");
            }
        }

        var commitResult = transactionScope.Commit();
        if (commitResult.IsSuccess)
        {
            await _cache.RemoveByPrefixAsync(prefixDepartmentKey, cancellationToken);
        }
    }

    private async Task<List<Department>> GetExpiredDepartments(CancellationToken cancellationToken)
    {
        var monthBefore = DateTime.UtcNow.AddMonths(-1 * Constants.EXPIRED_DEPARTMENTS_MONTHES);
        var departments = await _dbContext.Departments
            .Where(d => d.IsActive == false && d.DeletedAt.HasValue && d.DeletedAt.Value < monthBefore)
            .ToListAsync(cancellationToken);

        return departments;
    }

    private async Task<IReadOnlyList<Department>> GetDepartmentsWithLock(
        string[] departmentPaths, CancellationToken cancellationToken)
    {
        var departments = await _dbContext.Departments.FromSql(
            $"""
             SELECT 
                 id,
                 parent_id,
                 name,
                 identifier,
                 path,
                 depth,
                 is_active,
                 created_at,
                 updated_at,
                 deleted_at
             FROM departments WHERE path <@ ANY(ARRAY[{departmentPaths}]::ltree[]) FOR UPDATE
             """).ToListAsync(cancellationToken);

        return departments;
    }

    private async Task<Result> UpdateChildrenDepartments(
        string[] departmentPaths, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Database.ExecuteSqlAsync(
                $"""
                WITH updates AS (
                    SELECT
                        pd.id new_parent_id,
                        pd.path new_parent_path,
                        d.path old_path,
                        d.depth old_depth
                    FROM departments d
                    JOIN departments pd ON (d.parent_id = pd.id) 
                    WHERE d.path = ANY(ARRAY[{departmentPaths}]::ltree[])
                )
                
                UPDATE departments d
                SET
                    path = sd.new_parent_path || subpath(
                            d.path,
                            nlevel(sd.old_path),
                            nlevel(d.path) - nlevel(sd.old_path)
                           ),
                    depth = nlevel(sd.new_parent_path || subpath(
                            d.path,
                            nlevel(sd.old_path),
                            nlevel(d.path) - nlevel(sd.old_path))),
                    parent_id = sd.new_parent_id,
                    updated_at = now()
                FROM updates sd
                WHERE d.path <@ sd.old_path
                  AND d.path != sd.old_path
                """, cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Отмена операции обновления дочерних подразделений");
            return DepartmentErrors.DatabaseUpdateChildrenError();
        }
    }

    private async Task<Result> DeleteDepartments(string[] departmentPaths, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Database.ExecuteSqlAsync(
                $"""
                 DELETE FROM departments
                 WHERE path = ANY(ARRAY[{departmentPaths}]::ltree[]);
                 """, cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Отмена операции удаления подразделений");
            return DepartmentErrors.DatabaseDeleteError();
        }
    }
}