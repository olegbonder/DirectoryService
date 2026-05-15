using SharedKernel.Result;

namespace AuthService.Domain.Shared
{
    public static class TokenErrors
    {
        public static Error RefreshTokenNotFound() =>
            Error.Failure("refresh.token..not.found", "Refresh token not found");

        public static Error RefreshTokenExpired() =>
            Error.Failure("refresh.token.expired", "Refresh token expired");

        public static Error InvalidAccessToken() =>
            Error.Failure("jwt.invalid_token", "Invalid access token");
    }
}