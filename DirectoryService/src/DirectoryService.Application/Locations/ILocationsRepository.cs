using System.Linq.Expressions;
using DirectoryService.Domain.Locations;
using Shared.Result;

namespace DirectoryService.Application.Locations
{
    public interface ILocationsRepository
    {
        Task<Result<Guid>> AddAsync(Location location, CancellationToken cancellationToken);

        Task<Result<bool>> ExistsByAsync(Expression<Func<Location, bool>> predicate, CancellationToken cancellationToken);
    }
}
