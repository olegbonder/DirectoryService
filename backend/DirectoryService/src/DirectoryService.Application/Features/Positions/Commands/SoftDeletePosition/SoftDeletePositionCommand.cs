using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Features.Positions.Commands.SoftDeletePosition;

public record SoftDeletePositionCommand(Guid PositionId) : ICommand;