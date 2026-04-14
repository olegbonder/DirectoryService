using Core.Caching;
using FileService.Core;
using FileService.Infrastructure.S3;
using FileService.Presenters;
using FileService.VideoProcessing;
using Framework.Endpoints;
using Framework.Logging;
using Framework.Swagger;

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
                .AddApplication()
                .AddEndpoints(typeof(DependencyInjectionCore).Assembly)
                .AddCors()
                .AddS3(configuration)
                .AddVideoProcessing(configuration)
                .AddQuartzService()
                .AddProgressNotifier()
                .AddSignalR();

            return services;
        }
    }
}
