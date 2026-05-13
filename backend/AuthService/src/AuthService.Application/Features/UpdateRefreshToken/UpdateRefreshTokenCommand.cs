using AuthService.Contracts.Dtos.UpdateRefreshToken;
using Core.Abstractions;

namespace AuthService.Application.Features.UpdateRefreshToken;

public record UpdateRefreshTokenCommand(UpdateRefreshTokenRequest Request) : ICommand;