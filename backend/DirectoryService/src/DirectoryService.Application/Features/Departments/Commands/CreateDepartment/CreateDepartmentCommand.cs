using Core.Abstractions;
using DirectoryService.Contracts.Departments.CreateDepartment;

namespace DirectoryService.Application.Features.Departments.Commands.CreateDepartment
{
    public record CreateDepartmentCommand(CreateDepartmentRequest Request) : ICommand;
}
