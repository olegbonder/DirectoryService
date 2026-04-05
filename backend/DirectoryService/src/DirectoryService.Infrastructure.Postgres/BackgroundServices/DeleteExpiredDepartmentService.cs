using Core.Caching;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using DirectoryService.Infrastructure.Postgres.Database;
using IntegrationEvents.Directory.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace DirectoryService.Infrastructure.Postgres.BackgroundServices;

public class DeleteExpiredDepartmentService
{
    private readonly ILogger<DeleteExpiredDepartmentService> _logger;
    private readonly TransactionManager _transactionManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly ICacheService _cache;
    private readonly IOutboxService _outboxService;

    public DeleteExpiredDepartmentService(
        ILogger<DeleteExpiredDepartmentService> logger,
        TransactionManager transactionManager,
        ApplicationDbContext dbContext,
        ICacheService cache,
        IOutboxService outboxService)
    {
        _logger = logger;
        _transactionManager = transactionManager;
        _dbContext = dbContext;
        _cache = cache;
        _outboxService = outboxService;
    }

    public async Task Process(CancellationToken cancellationToken)
    {
        string prefixDepartmentKey = "departments_";
        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken: cancellationToken);
        if (transactionScopeResult.IsFailure)
        {
            return;
        }

        var departments = await GetExpiredDepartments(cancellationToken);
        if (departments.Any())
        {
            var updateChildResults = new List<Result>();
            string context = nameof(Department);
            foreach (var department in departments)
            {
                var departmentId = department.Id.Value;
                string departmentPath = department.Path.Value;
                var result = await UpdateChildrenDepartmentPaths(departmentPath, departmentId, cancellationToken);
                updateChildResults.Add(result);

                await UpdateChildrenDepartments(department.Id, cancellationToken);

                var videoId = department.VideoId;
                if (videoId.HasValue)
                {
                    var departmentDeletedEvent = new DepartmentDeleted(videoId.Value, context, departmentId);
                    await _outboxService.PublishAsync(departmentDeletedEvent);
                }

                var previewId = department.PreviewId;
                if (previewId.HasValue)
                {
                    var departmentDeletedEvent = new DepartmentDeleted(previewId.Value, context, departmentId);
                    await _outboxService.PublishAsync(departmentDeletedEvent);
                }
            }

            var failedResults = updateChildResults.Where(r => r.IsFailure).ToList();
            if (failedResults.Any())
            {
                await _transactionManager.RollbackAsync(cancellationToken);
            }

            var departmentIds = departments.Select(d => d.Id).ToList();

            await DeletePositionsByExpiredDepartments(departmentIds, cancellationToken);
            await DeleteLocationsByExpiredDepartments(departmentIds, cancellationToken);

            _dbContext.Departments.RemoveRange(departments);

            var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (saveResult.IsFailure)
            {
                await _transactionManager.RollbackAsync(cancellationToken);
                _logger.LogError("Отмена операции удаления подразделений/позиций/локаций");
            }
        }

        var commitResult = await _transactionManager.CommitTransactionAsync(cancellationToken);
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

    private async Task<Result> UpdateChildrenDepartmentPaths(
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
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Отмена операции обновления дочерних подразделений у родителя {newDepartmentId}", newDepartmentId);
            return DepartmentErrors.DatabaseUpdateChildrenError(newDepartmentId);
        }
    }
}