using DirectoryService.Contracts.Locations;
using Shared.Result;

namespace DirectoryService.Application.Locations
{
    public interface ILocationService
    {
        Task<Result<Guid>> Create(CreateLocationDTO locationDTO, CancellationToken cancellationToken);
    }
}