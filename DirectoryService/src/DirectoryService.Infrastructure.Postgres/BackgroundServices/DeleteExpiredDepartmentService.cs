using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Postgres.BackgroundServices;

public class DeleteExpiredDepartmentService
{
    private readonly ILogger<DeleteExpiredDepartmentService> _logger;
    private readonly ApplicationDbContext _dbContext;

    public DeleteExpiredDepartmentService(
        ILogger<DeleteExpiredDepartmentService> logger,
        ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task Process(CancellationToken cancellationToken)
    {
        var departments = await GetExpiredDepartments(cancellationToken);
        if (departments.Any())
        {
            foreach (var department in departments)
            {
                var departmentId = department.Id.Value;
                string departmentPath = department.Path.Value;
                await UpdateChildrenDepartmentPaths(departmentPath, departmentId, cancellationToken);
                await UpdateChildrenDepartments(department.Id, cancellationToken);
            }

            var departmentIds = departments.Select(d => d.Id).ToList();

            await DeletePositionsByExpiredDepartments(departmentIds, cancellationToken);
            await DeleteLocationsByExpiredDepartments(departmentIds, cancellationToken);

            _dbContext.Departments.RemoveRange(departments);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Отмена операции удаления подразделений/позиций/локаций");
            }
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

    private async Task UpdateChildrenDepartments(DepartmentId departmentId, CancellationToken cancellationToken)
    {
        await _dbContext.Departments
            .Where(d => d.ParentId == departmentId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(d => d.ParentId, (DepartmentId?)null),  cancellationToken);
    }

    private async Task DeleteLocationsByExpiredDepartments(
        List<DepartmentId> departmentIds,
        CancellationToken cancellationToken)
    {
        await _dbContext.Departments
            .Where(d => departmentIds.Contains(d.Id))
            .SelectMany(d => d.DepartmentLocations.Select(dl => dl.Location))
            .ExecuteDeleteAsync(cancellationToken);
    }

    private async Task DeletePositionsByExpiredDepartments(
        List<DepartmentId> departmentIds,
        CancellationToken cancellationToken)
    {
        await _dbContext.Positions
            .Where(p => p.DepartmentPositions.Any(d => departmentIds.Contains(d.DepartmentId)))
            .ExecuteDeleteAsync(cancellationToken);
    }

    private async Task UpdateChildrenDepartmentPaths(
        string oldDepartmentPath, Guid newDepartmentId, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Database.ExecuteSqlAsync(
                $"""
                 UPDATE departments
                 SET path = subpath(path, nlevel({oldDepartmentPath}::ltree), nlevel(path::ltree)),
                     depth = nlevel(path::ltree) - 1,
                     updated_at = now()
                 WHERE path <@ {oldDepartmentPath}::ltree
                 and path != {oldDepartmentPath}::ltree
                 """, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Отмена операции обновления дочерних подразделений у родителя {newDepartmentId}", newDepartmentId);
        }
    }
}