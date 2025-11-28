using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Features.Departments.Commands.MoveDepartment;

public record MoveDepartmentCommand(Guid DepartmentId, MoveDepartmentRequest Request) : ICommand;