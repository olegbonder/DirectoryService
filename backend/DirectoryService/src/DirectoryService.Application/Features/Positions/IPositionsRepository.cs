using System.Linq.Expressions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using Shared.Result;

namespace DirectoryService.Application.Features.Positions
{
    public interface IPositionsRepository
    {
        Task<Result<Guid>> AddAsync(Position position, CancellationToken cancellationToken);

        Task<Position?> GetBy(Expression<Func<Position, bool>> predicate, CancellationToken cancellationToken);

        Task<Result> DeactivatePositionsByDepartment(
            DepartmentId departmentId, CancellationToken cancellationToken);
    }
}
