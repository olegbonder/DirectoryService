using System.Linq.Expressions;
using DirectoryService.Domain.Departments;
using Shared.Result;

namespace DirectoryService.Application.Features.Departments
{
    public interface IDepartmentsRepository
    {
        Task<Result<Guid>> AddAsync(Department department, CancellationToken cancellationToken);

        Task<Department> GetBy(Expression<Func<Department, bool>> predicate, CancellationToken cancellationToken);
    }
}
