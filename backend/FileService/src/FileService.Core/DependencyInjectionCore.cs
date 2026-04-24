using Core.Abstractions;
using Core.Caching;
using FileService.Core.Features.CheckMediaAssetExists;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace FileService.Core
{
    public static class DependencyInjectionCore
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            var assembly = typeof(DependencyInjectionCore).Assembly;
            services.AddValidatorsFromAssembly(assembly);

            services.AddHandlers(assembly);

            services.AddScoped<CheckMediaAssetExistsHandler>();

            services.AddDistributedAndLocalCache(configuration);

            services.AddQuartzServices(configuration);

            return services;
        }

        private static IServiceCollection AddQuartzServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddQuartz(options => 
            {
                options.UsePersistentStore(persistenceOptions =>
                {
                    persistenceOptions.UsePostgres(cfg =>
                    {
                        cfg.ConnectionString = configuration.GetConnectionString(ConnectionStringNames.DATABASE)!;
                    });

                    persistenceOptions.UseNewtonsoftJsonSerializer();
                    persistenceOptions.UseProperties = true;
                });
            });

            services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

            return services;
        }
    }
}
