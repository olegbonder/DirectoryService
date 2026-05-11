using AuthService.Contracts.Dtos.Login;
using Core.Abstractions;

namespace AuthService.Application.Features.Login;

public record LoginUserCommand(LoginRequest Request) : ICommand;