using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Positions;

namespace DirectoryService.Application.Features.Positions.Commands.CreatePosition
{
    public record CreatePositionCommand(CreatePositionRequest Request) : ICommand;
}
