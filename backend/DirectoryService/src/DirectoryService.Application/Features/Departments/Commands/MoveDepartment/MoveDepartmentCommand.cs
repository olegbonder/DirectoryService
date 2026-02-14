using Core.Abstractions;
using DirectoryService.Contracts.Departments.MoveDepartment;

namespace DirectoryService.Application.Features.Departments.Commands.MoveDepartment;

public record MoveDepartmentCommand(Guid DepartmentId, MoveDepartmentRequest Request) : ICommand;