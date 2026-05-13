namespace AuthService.Contracts.Dtos.ResetPassword
{
    public record ResetPasswordRequest(Guid UserId, string Token, string NewPassword);
}