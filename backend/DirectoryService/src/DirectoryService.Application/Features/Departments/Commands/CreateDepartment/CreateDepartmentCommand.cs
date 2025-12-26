using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Departments.CreateDepartment;

namespace DirectoryService.Application.Features.Departments.Commands.CreateDepartment
{
    public record CreateDepartmentCommand(CreateDepartmentRequest Request) : ICommand;
}
