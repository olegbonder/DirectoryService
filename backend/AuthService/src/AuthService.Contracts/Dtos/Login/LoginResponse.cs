namespace AuthService.Contracts.Dtos.Login;

public record LoginResponse(string AccessToken, int ExpiresIn);