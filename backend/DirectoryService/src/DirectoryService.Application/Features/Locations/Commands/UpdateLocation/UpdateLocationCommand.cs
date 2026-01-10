using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations.CreateLocation;

namespace DirectoryService.Application.Features.Locations.Commands.UpdateLocation
{
    public record UpdateLocationCommand(Guid LocationId, UpdateLocationRequest Request) : ICommand;
}
