using AuthService.Contracts.Dtos.RegisterUser;
using Core.Abstractions;

namespace AuthService.Application.Features.RegisterUser;

public record RegisterUserCommand(RegisterUserRequest Request) : ICommand;