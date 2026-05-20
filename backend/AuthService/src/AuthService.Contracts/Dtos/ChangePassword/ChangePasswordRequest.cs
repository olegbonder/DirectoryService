namespace AuthService.Contracts.Dtos.ChangePassword
{
    public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
}