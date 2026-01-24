using System.Linq.Expressions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using Shared.Result;

namespace DirectoryService.Application.Features.Positions
{
    public interface IPositionsRepository
    {
        Task<Result<Guid>> Add(Position position, CancellationToken cancellationToken);

        Task<Result> Update(Position position, CancellationToken cancellationToken);

        Task<Position?> GetBy(Expression<Func<Position, bool>> predicate, CancellationToken cancellationToken);

        Task<Position?> GetActivePositionById(PositionId positionId, CancellationToken cancellationToken);

        Task<Position?> GetActivePositionByName(PositionName positionName, CancellationToken cancellationToken);

        Task<Result> DeactivatePositionsByDepartment(
            DepartmentId departmentId, CancellationToken cancellationToken);

        Task<Result> DeactivatePosition(PositionId positionId, CancellationToken cancellationToken);
    }
}
