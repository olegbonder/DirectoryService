using AuthService.Contracts.Dtos.ForgotPassword;
using Core.Abstractions;

namespace AuthService.Application.Features.ForgotPassword;

public record ForgotPasswordCommand(ForgotPasswordRequest Request) : ICommand;