using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Features.Departments.Commands.SoftDeleteDepartment;

public record SoftDeleteDepartmentCommand(Guid DepartmentId) : ICommand;