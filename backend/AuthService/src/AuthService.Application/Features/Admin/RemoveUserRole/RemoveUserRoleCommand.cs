using Core.Abstractions;

namespace AuthService.Application.Features.Admin.RemoveUserRole;

public record RemoveUserRoleCommand(Guid UserId, string Role) : ICommand;