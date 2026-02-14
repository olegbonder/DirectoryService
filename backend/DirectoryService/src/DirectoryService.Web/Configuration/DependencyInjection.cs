using Core.Caching;
using DirectoryService.Application;
using Framework.Logging;
using Framework.Swagger;

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
                .AddControllers();

            return services;
        }
    }
}
