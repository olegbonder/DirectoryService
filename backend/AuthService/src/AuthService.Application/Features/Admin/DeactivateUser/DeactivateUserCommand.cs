using Core.Abstractions;

namespace AuthService.Application.Features.Admin.DeactivateUser;

public record DeactivateUserCommand(Guid UserId) : ICommand;