using AuthService.Contracts.Dtos.VerifyEmail;
using Core.Abstractions;

namespace AuthService.Application.Features.VerifyEmail;

public record VerifyEmailCommand(VerifyEmailRequest Request) : ICommand;