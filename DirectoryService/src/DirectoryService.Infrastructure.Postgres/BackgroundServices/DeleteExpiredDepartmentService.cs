using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using DirectoryService.Infrastructure.Postgres.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Result;

namespace DirectoryService.Infrastructure.Postgres.BackgroundServices;

public class DeleteExpiredDepartmentService
{
    private readonly ILogger<DeleteExpiredDepartmentService> _logger;
    private readonly TransactionManager _transactionManager;
    private readonly ApplicationDbContext _dbContext;

    public DeleteExpiredDepartmentService(
        ILogger<DeleteExpiredDepartmentService> logger,
        TransactionManager transactionManager,
        ApplicationDbContext dbContext)
    {
        _logger = logger;
        _transactionManager = transactionManager;
        _dbContext = dbContext;
    }

    public async Task Process(CancellationToken cancellationToken)
    {
        var transactionScopeResult = await _transactionManager.BeginTransaction(cancellationToken: cancellationToken);
        if (transactionScopeResult.IsFailure)
        {
            return;
        }

        using var transactionScope = transactionScopeResult.Value;

        var departments = await GetExpiredDepartments(cancellationToken);
        if (departments.Any())
        {
            var departmentPaths = departments.Select(d => d.Path).ToList();
            var lockDepartments = await GetDepartmentsWithLock(departmentPaths, cancellationToken);
            if (lockDepartments.Count != departments.Count)
            {
                transactionScope.RollBack();
                _logger.LogError("Отмена операции удаления подразделений из-за несовпадения количества заблокированных записей");
                return;
            }

            var updateChildResults = new List<Result>();
            foreach (var department in departments)
            {
                var result = await UpdateChildrenDepartments(department, cancellationToken);
                updateChildResults.Add(result);
            }

            var failedResults = updateChildResults.Where(r => r.IsFailure).ToList();
            if (failedResults.Any())
            {
                transactionScope.RollBack();
                var errors = failedResults.SelectMany(r => r.Errors.Select(e => e.Message)).ToList();
                _logger.LogError(string.Join(", ", errors));
                return;
            }

            _dbContext.Departments.RemoveRange(departments);

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

        transactionScope.Commit();
    }

    private async Task<List<Department>> GetExpiredDepartments(CancellationToken cancellationToken)
    {
        var monthBefore = DateTime.UtcNow.AddMonths(-1 * Constants.EXPIRED_DEPARTMENTS_MONTHES);
        var departments = await _dbContext.Departments
            .Where(d => d.IsActive == false && d.DeletedAt.HasValue && d.DeletedAt.Value < monthBefore)
            .ToListAsync(cancellationToken);

        return departments;
    }

    public async Task<IReadOnlyList<Department>> GetDepartmentsWithLock(
        List<DepartmentPath> departmentPaths, CancellationToken cancellationToken)
    {
        var departmentPathsString = departmentPaths.Select(dp => dp.Value).ToArray();
        var childrens = await _dbContext.Departments.FromSql(
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
             FROM departments WHERE path <@ ANY(ARRAY[{departmentPathsString}]::ltree[]) FOR UPDATE
             """).ToListAsync(cancellationToken);

        return childrens;
    }

    private async Task<Result> UpdateChildrenDepartments(
        Department department, CancellationToken cancellationToken)
    {
        var departmentId = department.Id.Value;
        string departmentPath = department.Path.Value;
        var parentDepartmentId = department.ParentId?.Value;
        try
        {
            await _dbContext.Database.ExecuteSqlAsync(
                $"""
                 UPDATE departments
                 SET path = subpath(path, nlevel({departmentPath}::ltree), nlevel(path::ltree)),
                     depth = nlevel(path::ltree) - 1,
                     parent_id = {parentDepartmentId},
                     updated_at = now()
                 WHERE path <@ {departmentPath}::ltree
                 and path != {departmentPath}::ltree
                 """, cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Отмена операции обновления дочерних подразделений у родителя {departmentId}", departmentId);
            return DepartmentErrors.DatabaseUpdateChildrenError(departmentId);
        }
    }
}