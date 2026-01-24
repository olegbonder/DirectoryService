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

        public async Task<Department?> GetBy(
            Expression<Func<Department, bool>> predicate, CancellationToken cancellationToken) =>
            await _context.Departments.FirstOrDefaultAsync(predicate, cancellationToken);

        public async Task<Department?> GetByWithLocations(
            Expression<Func<Department, bool>> predicate, CancellationToken cancellationToken) =>
            await _context.Departments
                .Include(d => d.DepartmentLocations)
                .FirstOrDefaultAsync(predicate, cancellationToken);

        public async Task<Result<IReadOnlyCollection<Department>>> GetDepartmentByIds(
            IEnumerable<DepartmentId> departmentIds, CancellationToken cancellationToken)
        {
            var departments = await _context.Departments
                .Where(d => departmentIds.Contains(d.Id))
                .ToListAsync(cancellationToken);

            var notFoundDepartmentIds = departmentIds
                .Except(departments.Select(d => d.Id)).ToList();
            if (notFoundDepartmentIds.Any())
            {
                var errors = notFoundDepartmentIds
                    .Select(d => DepartmentErrors.NotFound(d.Value));

                return new Errors(errors);
            }

            return departments;
        }

        public async Task<Result<Department?>> GetByIdWithLock(
            DepartmentId departmentId, CancellationToken cancellationToken)
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
                    updated_at,
                    deleted_at
                FROM departments WHERE id = {id} AND is_active FOR UPDATE
                """).FirstOrDefaultAsync(cancellationToken);

            if (department == null)
            {
                return DepartmentErrors.NotFound(id);
            }

            return department;
        }

        public async Task<IReadOnlyList<Department>> GetChildrensWithLock(
            DepartmentPath parentPath, CancellationToken cancellationToken)
        {
            string parentPathValue = parentPath.Value;
            var childrens = await _context.Departments.FromSql(
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
                FROM departments WHERE path <@ {parentPathValue}::ltree FOR UPDATE
                """).ToListAsync(cancellationToken);

            return childrens;
        }

        public async Task<bool> IsExistsChildForParent(
            DepartmentId id, DepartmentId parentId, CancellationToken cancellationToken) =>
            await _context.Departments.AnyAsync(
                d => d.Id == id &&
                   d.Children.Any(c => c.Id == parentId), cancellationToken);

        public async Task<Result> UpdateChildrensForMove(
            DepartmentPath oldDepartmentPath, Department department, CancellationToken cancellationToken)
        {
            string oldDepartmentPathValue = oldDepartmentPath.Value;
            string newDepatmentPathValue = department.Path.Value;
            int newDepatmentDepth = department.Depth;
            try
            {
                await _context.Database.ExecuteSqlAsync(
                    $"""
                    UPDATE departments
                    SET depth = {newDepatmentDepth} + nlevel(subpath(path, nlevel({oldDepartmentPathValue}::ltree), nlevel(path::ltree))),
                    path = ({newDepatmentPathValue}::ltree || subpath(path, nlevel({oldDepartmentPathValue}::ltree), nlevel(path::ltree))),
                    updated_at = now()
                    WHERE path <@ {oldDepartmentPathValue}::ltree
                    and path != {oldDepartmentPathValue}::ltree
                    """, cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                var deptId = department.Id.Value;
                _logger.LogError(ex, "Отмена операции обновления дочерних подразделений у родителя {deptId}", deptId);
                return DepartmentErrors.DatabaseUpdateChildrenError(deptId);
            }
        }

        public async Task<Result> UpdateChildrenAndParentPaths(
            string oldDepartmentPath, string newDepartmentPath, Guid newDepartmentId, CancellationToken cancellationToken)
        {
            try
            {
                await _context.Database.ExecuteSqlAsync(
                    $"""
                     UPDATE departments
                     SET path = ({newDepartmentPath}::ltree || subpath(path, nlevel({oldDepartmentPath}::ltree), nlevel(path::ltree))),
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

        public async Task<Department?> GetActiveDepartmentById(DepartmentId departmentId, CancellationToken cancellationToken) =>
            await GetBy(d => d.IsActive && d.Id == departmentId, cancellationToken);
    }
}
