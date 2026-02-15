using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using SharedKernel.Result;

namespace DirectoryService.Application.Features.Locations
{
    public interface ILocationsRepository
    {
        Task<Result<Guid>> Add(Location location, CancellationToken cancellationToken);

        Task<Result> Update(Location location, CancellationToken cancellationToken);

        Task<Result<IReadOnlyCollection<Location>>> GetLocationsByIds(
            List<LocationId> locationIds, CancellationToken cancellationToken);

        Task<Location?> GetActiveLocationById(
            LocationId locationId, CancellationToken cancellationToken);

        Task<Result<IReadOnlyCollection<Location>>> GetActiveLocationsByIds(
            List<LocationId> locationIds, CancellationToken cancellationToken);

        Task<Result> DeactivateLocationsByDepartment(
            DepartmentId departmentId, CancellationToken cancellationToken);

        Task<Result> UpdateLocation(Location location, CancellationToken cancellationToken);

        Task<Result> DeactivateLocation(LocationId locationId, CancellationToken cancellationToken);
    }
}
