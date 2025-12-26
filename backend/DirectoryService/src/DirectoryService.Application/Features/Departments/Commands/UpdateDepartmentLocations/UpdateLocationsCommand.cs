using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments.UpdateDepartmentLocations;

namespace DirectoryService.Application.Features.Departments.Commands.UpdateDepartmentLocations
{
    public record UpdateLocationsCommand(Guid DepartmentId, UpdateDepartmentLocationsRequest Request) : ICommand;
}
