using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Core.Caching;

public class DistributedCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, bool> _cacheKeys = new();
    private readonly IDistributedCache _cache;

    public DistributedCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetOrSetAsync<T>(string key, DistributedCacheEntryOptions options, Func<Task<T?>> factory,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var cachedValue = await GetAsync<T>(key, options, cancellationToken);
        if (cachedValue != null)
            return cachedValue;

        var freshValue = await factory();
        if (freshValue != null)
        {
            await SetAsync(key, freshValue, options, cancellationToken);
        }

        return freshValue;
    }

    public async Task<T?> GetAsync<T>(string key, DistributedCacheEntryOptions options,
        CancellationToken cancellationToken = default)
        where T : class
    {
        string? cachedValue = await _cache.GetStringAsync(key, cancellationToken);

        return cachedValue is null
            ? null
            : JsonSerializer.Deserialize<T>(cachedValue);
    }

    public async Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options,
        CancellationToken cancellationToken = default)
        where T : class
    {
        string cachedValue = JsonSerializer.Serialize(value);

        await _cache.SetStringAsync(key, cachedValue, options, cancellationToken);

        _cacheKeys.TryAdd(key, true);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);

        _cacheKeys.TryRemove(key, out bool _);
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        var tasks = _cacheKeys
            .Keys
            .Where(k => k.StartsWith(prefix))
            .Select(k => RemoveAsync(k, cancellationToken));

        await Task.WhenAll(tasks);
    }
}