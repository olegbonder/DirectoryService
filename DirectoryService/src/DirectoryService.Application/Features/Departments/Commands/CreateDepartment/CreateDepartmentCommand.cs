using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Features.Departments.Commands.CreateDepartment
{
    public record CreateDepartmentCommand(CreateDepartmentRequest Request) : ICommand;
}
