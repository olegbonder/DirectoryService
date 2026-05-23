using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SharedAuth.DevAuth
{
    public static class DevAuthExtensions
    {
        public static IServiceCollection AddDevAuth(this IServiceCollection services, IConfiguration configuration) =>
            services.Configure<DevAuthOptions>(configuration.GetSection(DevAuthOptions.SECTION_NAME));

        public static IApplicationBuilder UseDevAuth(this IApplicationBuilder builder) =>
            builder.UseMiddleware<DevAuthMiddleware>();
    }
}