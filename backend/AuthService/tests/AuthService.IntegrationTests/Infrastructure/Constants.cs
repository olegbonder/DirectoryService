namespace AuthService.IntegrationTests.Infrastructure;

public static class Constants
{
    public const string BASE_URL = "/api";
    public const string REGISTER_USER_URL = $"{BASE_URL}/auth/register";

    public const string CONFIRM_EMAIL_URL = "/api/auth/confirm-email";
    public const string LOGIN_URL = $"{BASE_URL}/auth/login";
    public const string REFRESH_ACCESS_TOKEN_URL = $"{BASE_URL}/auth/refresh";
    public const string PROTECTED_ENDPOINT_URL = $"{BASE_URL}/auth/me"; // Protected endpoint - returns user info
    public const string ADMIN_PROTECTED_ENDPOINT_URL = $"{BASE_URL}/users"; // Admin-protected endpoint - requires users.manage permission
}