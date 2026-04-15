using Core.Abstractions;
using Core.Caching;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.Core
{
    public static class DependencyInjectionCore
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            var assembly = typeof(DependencyInjectionCore).Assembly;
            services.AddValidatorsFromAssembly(assembly);

            services.AddHandlers(assembly);

            services.AddDistributedAndLocalCache(configuration);/*.AddStackExchangeRedisCache(setup =>
            {
                setup.Configuration = "localhost:6379";
            });
            services.AddHybridCache(options =>
            {
                options.DefaultEntryOptions = new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(30),
                    LocalCacheExpiration = TimeSpan.FromMinutes(5),
                };
            });*/

            return services;
        }
    }
}
