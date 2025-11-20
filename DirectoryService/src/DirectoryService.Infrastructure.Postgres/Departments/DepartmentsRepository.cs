using System.Linq.Expressions;
using DirectoryService.Application.Features.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Result;

namespace DirectoryService.Infrastructure.Postgres.Departments
{
    public class DepartmentsRepository : IDepartmentsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DepartmentsRepository> _logger;

        public DepartmentsRepository(ApplicationDbContext context, ILogger<DepartmentsRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<Guid>> AddAsync(Department department, CancellationToken cancellationToken)
        {
            var name = department.Name.Value;
            try
            {
                await _context.Departments.AddAsync(department, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return department.Id.Value;
            }
            catch(OperationCanceledException ex)
            {
                _logger.LogError(ex, "Отмена операции добавления подразделения с наименованием {name}", name);
                return DepartmentErrors.OperationCancelled();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка добавления подразделения с наименованием {name}", name);
                return DepartmentErrors.DatabaseError();
            }
        }

        public async Task<Department?> GetBy(Expression<Func<Department, bool>> predicate, CancellationToken cancellationToken) =>
            await _context.Departments.FirstOrDefaultAsync(predicate, cancellationToken);

        public async Task<Result<IReadOnlyCollection<Department>>> GetDepartmentByIds(List<DepartmentId> departmentIds, CancellationToken cancellationToken)
        {
            var departments = await _context.Departments.Where(d => departmentIds.Contains(d.Id)).ToListAsync(cancellationToken);
            var notFoundDepartmentIds = departmentIds.Except(departments.Select(d => d.Id));
            if (notFoundDepartmentIds.Any())
            {
                var errors = notFoundDepartmentIds.Select(d => DepartmentErrors.NotFound(d.Value));

                return new Errors(errors);
            }

            return departments;
        }

        public async Task<Result> DeleteLocationsByDepartment(DepartmentId deptId, CancellationToken cancellationToken)
        {
            await _context.DepartmentLocations.Where(d => d.DepartmentId == deptId).ExecuteDeleteAsync(cancellationToken);

            return Result.Success();
        }

        public async Task SaveChanges(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result<Department?>> GetByIdWithLock(DepartmentId departmentId, CancellationToken cancellationToken)
        {
            var id = departmentId.Value;
            var department = await _context.Departments.FromSql(
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
                    updated_at 
                FROM departments WHERE id = {id} AND is_active FOR UPDATE
                """).FirstOrDefaultAsync(cancellationToken);

            return department;
        }

        public async Task<bool> IsExistsChildForParent(DepartmentId id, DepartmentId parentId, CancellationToken cancellationToken) =>
            await _context.Departments.AnyAsync(
                d => d.Id == id &&
                   d.Children.Any(c => c.Id == parentId), cancellationToken);
    }
}
