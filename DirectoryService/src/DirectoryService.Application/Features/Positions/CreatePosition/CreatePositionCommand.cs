using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Positions;

namespace DirectoryService.Application.Features.Positions.CreatePosition
{
    public record CreatePositionCommand(CreatePositionRequest Request) : ICommand;
}
