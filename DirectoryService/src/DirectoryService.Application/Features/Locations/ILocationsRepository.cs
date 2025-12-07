using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Shared.Result;

namespace DirectoryService.Application.Features.Locations
{
    public interface ILocationsRepository
    {
        Task<Result<Guid>> AddAsync(Location location, CancellationToken cancellationToken);

        Task<Result<IReadOnlyCollection<Location>>> GetLocationsByIds(
            List<LocationId> locationIds, CancellationToken cancellationToken);

        Task<Result<IReadOnlyCollection<Location>>> GetActiveLocationsByIds(
            List<LocationId> locationIds, CancellationToken cancellationToken);

        Task<Result> DeactivateLocationsByDepartment(
            DepartmentId departmentId, CancellationToken cancellationToken);
    }
}
