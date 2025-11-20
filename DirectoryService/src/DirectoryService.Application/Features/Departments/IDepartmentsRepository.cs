using System.Linq.Expressions;
using DirectoryService.Domain.Departments;
using Shared.Result;

namespace DirectoryService.Application.Features.Departments
{
    public interface IDepartmentsRepository
    {
        Task<Result<Guid>> AddAsync(Department department, CancellationToken cancellationToken);

        Task<Department?> GetBy(Expression<Func<Department, bool>> predicate, CancellationToken cancellationToken);

        Task<bool> IsExistsChildForParent(DepartmentId id, DepartmentId parentId, CancellationToken cancellationToken);

        Task<Result<IReadOnlyCollection<Department>>> GetDepartmentByIds(List<DepartmentId> departmentIds, CancellationToken cancellationToken);

        Task<Result> DeleteLocationsByDepartment(DepartmentId deptId, CancellationToken cancellationToken);

        Task<Result<Department?>> GetByIdWithLock(DepartmentId departmentId, CancellationToken cancellationToken);
    }
}
