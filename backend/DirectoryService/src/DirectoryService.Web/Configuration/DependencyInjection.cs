using Core.Caching;
using DirectoryService.Application;
using FileService.Contracts.HttpCommunication;
using Framework.Logging;
using Framework.Swagger;
using SharedAuth.DevAuth;
using SharedAuth.Jwt;
using SharedAuth.Permissions;

namespace DirectoryService.Web.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddProgramDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddSerilogLogging(configuration, "DirectoryService")
                .AddDistributedCache(configuration)
                .AddOpenApiSpec()
                .AddApplication()
                .AddCors()
                .AddJwtAuthentication(configuration)
                .AddPermissionAuthorization()
                .AddDevAuth(configuration)
                .AddFileHttpCommunication(configuration)
                .AddControllers();

            return services;
        }
    }
}
