using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments.CreateDepartment;

namespace DirectoryService.Application.Features.Departments.Commands.UpdateDepartment
{
    public record UpdateDepartmentCommand(Guid Id, CreateDepartmentRequest Request) : ICommand;
}
