using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Features.Departments.UpdateDepartmentLocations
{
    public record UpdateLocationsCommand(Guid DepartmentId, UpdateDepartmentLocationsRequest Request) : ICommand;
}
