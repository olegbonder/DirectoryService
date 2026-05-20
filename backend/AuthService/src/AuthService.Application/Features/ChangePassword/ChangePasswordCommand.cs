using AuthService.Contracts.Dtos.ChangePassword;
using Core.Abstractions;

namespace AuthService.Application.Features.ChangePassword;

public record ChangePasswordCommand(ChangePasswordRequest Request) : ICommand;