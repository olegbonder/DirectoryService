using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Application.Locations
{
    public interface ILocationService
    {
        Task<Result<Guid>> Create(CreateLocationDTO locationDTO, CancellationToken cancellationToken);
    }
}