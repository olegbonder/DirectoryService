using Core.Abstractions;
using DirectoryService.Contracts.Positions.CreatePositionDepartments;

namespace DirectoryService.Application.Features.Positions.Commands.CreatePositionDepartments
{
    public record CreatePositionDepartmentsCommand(Guid PositionId, CreatePositionDepartmentsRequest Request) : ICommand;
}
