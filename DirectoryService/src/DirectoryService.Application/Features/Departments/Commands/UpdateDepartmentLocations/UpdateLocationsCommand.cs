using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Features.Departments.Commands.UpdateDepartmentLocations
{
    public record UpdateLocationsCommand(Guid DepartmentId, UpdateDepartmentLocationsRequest Request) : ICommand;
}
