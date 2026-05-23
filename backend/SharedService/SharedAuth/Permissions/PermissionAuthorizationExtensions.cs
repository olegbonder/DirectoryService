using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using SharedAuth.UserScope;

namespace SharedAuth.Permissions
{
    public static class PermissionAuthorizationExtensions
    {
        public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
        {
            services.AddUserScopedData();
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, PermissionRequirementHandler>();

            return services;
        }

        public static IServiceCollection AddUserScopedData(this IServiceCollection services)
        {
            services.AddScoped<IUserScopedData, UserScopedData>();
            return services;
        }
    }
}