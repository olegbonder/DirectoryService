using Core.Abstractions;

namespace DirectoryService.Application.Features.Positions.Commands.DeletePositionDepartment
{
    public record DeletePositionDepartmentCommand(Guid PositionId, Guid DepartmentId) : ICommand;
}
