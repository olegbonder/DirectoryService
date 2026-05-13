namespace AuthService.Application.Model;

public record AccessToken(string Token, int ExpiresAt);