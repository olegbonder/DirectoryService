namespace AuthService.Contracts.Dtos.VerifyEmail;

public record VerifyEmailRequest(Guid UserId, string Token);