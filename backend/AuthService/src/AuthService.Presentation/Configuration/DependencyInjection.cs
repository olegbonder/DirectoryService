using AuthService.Application;
using Core.Caching;
using Framework.Endpoints;
using Framework.Logging;
using Framework.Swagger;

namespace AuthService.Presentation.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddProgramDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddSerilogLogging(configuration, "FileService")
                .AddDistributedCache(configuration)
                .AddOpenApiSpec()
                .AddCore(configuration)
                .AddEndpoints(typeof(DependencyInjectionCore).Assembly)
                .AddCors();

            return services;
        }
    }
}
