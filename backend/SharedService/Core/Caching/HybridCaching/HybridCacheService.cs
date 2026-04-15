using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;

namespace Core.Caching.HybridCaching;

public class HybridCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, bool> _cacheKeys = new();
    private readonly HybridCache _cache;

    public HybridCacheService(HybridCache cache)
    {
        _cache = cache;
    }

    public Task<T?> GetOrSetAsync<T>(string key, DistributedCacheEntryOptions options, Func<Task<T?>> factory,
        CancellationToken cancellationToken = default)
        where T : class
    {
        return _cache.GetOrCreateAsync(
            key,
            async cancel => await factory(),
            cancellationToken: cancellationToken).AsTask();
    }

    public async Task<T?> GetAsync<T>(string key, DistributedCacheEntryOptions options,
        CancellationToken cancellationToken = default)
        where T : class
    {
        return await _cache.GetOrCreateAsync(
            key,
            async cancel => null as T,
            cancellationToken: cancellationToken);
    }

    public Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options,
        CancellationToken cancellationToken = default)
        where T : class
    {
        _cacheKeys.TryAdd(key, true);
        return _cache.SetAsync(key, value, cancellationToken: cancellationToken).AsTask();
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cacheKeys.TryRemove(key, out _);
        return _cache.RemoveAsync(new[] { key }, cancellationToken).AsTask();
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        var tasks = _cacheKeys
            .Keys
            .Where(k => k.StartsWith(prefix, StringComparison.Ordinal))
            .Select(k => RemoveAsync(k, cancellationToken));

        return Task.WhenAll(tasks);
    }
}
