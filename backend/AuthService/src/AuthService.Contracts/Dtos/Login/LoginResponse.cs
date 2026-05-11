namespace AuthService.Contracts.Dtos.Login;

public record LoginResponse(string AccessToken, string RefreshToken, int ExpiresIn);