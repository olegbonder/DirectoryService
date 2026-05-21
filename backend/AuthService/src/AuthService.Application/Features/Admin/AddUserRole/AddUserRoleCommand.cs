using AuthService.Contracts.Dtos.Admin.AddUserRole;
using Core.Abstractions;

namespace AuthService.Application.Features.Admin.AddUserRole;

public record AddUserRoleCommand(Guid UserId, AddUserRoleRequest Request) : ICommand;