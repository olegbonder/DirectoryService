using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations;

namespace DirectoryService.Application.Features.Locations.Commands.CreateLocation
{
    public record CreateLocationCommand(CreateLocationRequest Request) : ICommand;
}
