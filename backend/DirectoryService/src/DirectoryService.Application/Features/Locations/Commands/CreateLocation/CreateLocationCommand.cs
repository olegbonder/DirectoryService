using Core.Abstractions;
using DirectoryService.Contracts.Locations.CreateLocation;

namespace DirectoryService.Application.Features.Locations.Commands.CreateLocation
{
    public record CreateLocationCommand(CreateLocationRequest Request) : ICommand;
}
