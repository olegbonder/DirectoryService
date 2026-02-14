using Core.Abstractions;
using DirectoryService.Contracts.Departments.UpdateDepartment;

namespace DirectoryService.Application.Features.Departments.Commands.UpdateDepartment
{
    public record UpdateDepartmentCommand(Guid Id, UpdateDepartmentRequest Request) : ICommand;
}
