using Core.Caching.HybridCaching;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Caching;

public static class CachingExtensions
{
    public static IServiceCollection AddDistributedCache(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            string connection = configuration.GetConnectionString("Redis")
                                ?? throw new ArgumentNullException(nameof(connection));

            options.Configuration = connection;
        });

        services.AddSingleton<ICacheService, DistributedCacheService>();
        return services;
    }

    public static IServiceCollection AddDistributedAndLocalCache(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDistributedCache(configuration);

        var hybridOptions = configuration.GetSection(nameof(HybridCachingOptions)).Get<HybridCachingOptions>()
                            ?? throw new ArgumentNullException(nameof(HybridCachingOptions));

        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(hybridOptions.DistributedCacheExpirationMinutes),
                LocalCacheExpiration = TimeSpan.FromMinutes(hybridOptions.LocalCacheExpirationMinutes),
            };
        });

        services.AddSingleton<ICacheService, HybridCacheService>();
        return services;
    }
}