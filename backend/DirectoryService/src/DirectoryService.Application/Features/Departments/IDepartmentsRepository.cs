using System.Linq.Expressions;
using DirectoryService.Domain.Departments;
using SharedKernel.Result;

namespace DirectoryService.Application.Features.Departments
{
    public interface IDepartmentsRepository
    {
        Task<Result<Guid>> AddAsync(Department department, CancellationToken cancellationToken);

        Task<Department?> GetBy(Expression<Func<Department, bool>> predicate, CancellationToken cancellationToken);

        Task<Department?> GetActiveDepartmentById(DepartmentId departmentId, CancellationToken cancellationToken);

        Task<IReadOnlyList<Department>> GetActiveDepartmentsByIds(
            List<DepartmentId> departmentIds, CancellationToken cancellationToken);

        Task<Department?> GetByWithLocations(
            Expression<Func<Department, bool>> predicate, CancellationToken cancellationToken);

        Task<bool> IsExistsChildForParent(DepartmentId id, DepartmentId parentId, CancellationToken cancellationToken);

        Task<Result<IReadOnlyCollection<Department>>> GetDepartmentByIds(
            IEnumerable<DepartmentId> departmentIds, CancellationToken cancellationToken);

        Task<Result<Department?>> GetByIdWithLock(DepartmentId departmentId, CancellationToken cancellationToken);

        Task<IReadOnlyList<Department>> GetChildrensWithLock(
            DepartmentPath parentPath, CancellationToken cancellationToken);

        Task<Result> UpdateChildrensForMove(
            DepartmentPath oldDepartmentPath, Department depatment, CancellationToken cancellationToken);

        Task<Result> UpdateChildrenPaths(
            string oldDepartmentPath, string newDepartmentPath, Guid newDepartmentId, CancellationToken cancellationToken);
    }
}
