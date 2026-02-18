using Core.Caching;
using FileService.Core;
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
                .AddCors();

            return services;
        }
    }
}
