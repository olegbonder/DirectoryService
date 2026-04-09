using Core.Abstractions;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.Core
{
    public static class DependencyInjectionCore
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = typeof(DependencyInjectionCore).Assembly;
            services.AddValidatorsFromAssembly(assembly);

            services.AddHandlers(assembly);

            services.AddStackExchangeRedisCache(setup =>
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
            });

            return services;
        }
    }
}
