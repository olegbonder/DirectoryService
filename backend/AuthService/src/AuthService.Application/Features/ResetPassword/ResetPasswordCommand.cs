using AuthService.Contracts.Dtos.ResetPassword;
using Core.Abstractions;

namespace AuthService.Application.Features.ResetPassword;

public record ResetPasswordCommand(ResetPasswordRequest Request) : ICommand;