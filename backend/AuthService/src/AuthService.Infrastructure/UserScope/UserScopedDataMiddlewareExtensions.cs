using Microsoft.AspNetCore.Builder;

namespace AuthService.Infrastructure.UserScope
{
    public static class UserScopedDataMiddlewareExtensions
    {
        public static IApplicationBuilder UseUserScopedData(this IApplicationBuilder builder) =>
            builder.UseMiddleware<UserScopedDataMiddleware>();
    }
}