using AuthService.Contracts.Dtos.Logout;
using Core.Abstractions;

namespace AuthService.Application.Features.Logout;

public record LogoutCommand(LogoutRequest Request) : ICommand;