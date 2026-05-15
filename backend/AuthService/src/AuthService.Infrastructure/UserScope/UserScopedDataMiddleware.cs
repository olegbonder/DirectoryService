using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.UserScope
{
    public class UserScopedDataMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UserScopedDataMiddleware> _logger;

        public UserScopedDataMiddleware(RequestDelegate next, ILogger<UserScopedDataMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, UserScopedData userScopedData)
        {
            var user = context.User;

            if (user?.Identity?.IsAuthenticated == true)
            {
                userScopedData.Authenticate(user);
                _logger.LogInformation(
                    "User authenticated: {UserId}, {Email}",
                    userScopedData.UserId,
                    userScopedData.Email);
            }
            else
            {
                _logger.LogWarning("User not authenticated");
            }

            await _next(context);
        }
    }
}