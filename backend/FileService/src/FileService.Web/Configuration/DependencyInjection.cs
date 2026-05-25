using Core.Caching;
using FileService.Core;
using FileService.Infrastructure.S3;
using FileService.Presenters;
using FileService.VideoProcessing;
using Framework.Endpoints;
using Framework.Logging;
using Framework.Swagger;
using SharedAuth.DevAuth;
using SharedAuth.Jwt;
using SharedAuth.Permissions;

namespace FileService.Web.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddProgramDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddSerilogLogging(configuration, "FileService")
                .AddDistributedCache(configuration)
                .AddOpenApiSpec()
                .AddApplication(configuration)
                .AddEndpoints(typeof(DependencyInjectionCore).Assembly)
                .AddJwtAuthentication(configuration)
                .AddPermissionAuthorization()
                .AddDevAuth(configuration)
                .AddCors()
                .AddS3(configuration)
                .AddVideoProcessing(configuration)
                .AddProgressNotifier()
                .AddSignalR();

            return services;
        }
    }
}
