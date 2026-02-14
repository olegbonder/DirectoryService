using Core.Abstractions;
using DirectoryService.Contracts.Positions.CreatePosition;

namespace DirectoryService.Application.Features.Positions.Commands.CreatePosition
{
    public record CreatePositionCommand(CreatePositionRequest Request) : ICommand;
}
