using Core.Abstractions;
using DirectoryService.Contracts.Locations.UpdatePosition;

namespace DirectoryService.Application.Features.Positions.Commands.UpdatePosition
{
    public record UpdatePositionCommand(Guid PositionId, UpdatePositionRequest Request) : ICommand;
}
