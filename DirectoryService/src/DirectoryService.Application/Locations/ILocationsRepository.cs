using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Application.Locations
{
    public interface ILocationsRepository
    {
        Task<Result<Guid>> AddAsync(Location location, CancellationToken cancellationToken);
    }
}
