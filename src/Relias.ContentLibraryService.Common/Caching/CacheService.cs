using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Relias.ContentLibraryService.Common.Caching;

/// <summary>
/// Basic cache service implementation
/// </summary>
public class CacheService(IDistributedCache distributedCache, ILogger<CacheService> logger) : ICacheService
{
    private readonly IDistributedCache _distributedCache = distributedCache;
    private readonly ILogger<CacheService> _logger = logger;

    /// <summary>
    /// Read from cache
    /// </summary>
    public async Task<T?> GetAsync<T>(string key)
    {
        string fullKey = GetFullKey<T>(key);

        string? json = await _distributedCache.GetStringAsync(fullKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            _logger.LogTrace("Cache miss '{FullKey}'", fullKey);
            return default;
        }

        _logger.LogTrace("Cache hit '{FullKey}'", fullKey);
        return JsonSerializer.Deserialize<T>(json);
    }

    /// <summary>
    /// Store value to cache
    /// </summary>
    public async Task SetAsync<T>(string key, T obj)
    {
        await SetInternalAsync(key, obj);
    }

    /// <summary>
    /// Store value to cache
    /// </summary>
    public async Task SetAsync<T>(string key, T obj, Func<DistributedCacheEntryOptions> optionsFunc)
    {
        await SetInternalAsync(key, obj, optionsFunc?.Invoke());
    }

    /// <summary>
    /// Remove value from cache
    /// </summary>
    public async Task RemoveAsync<T>(string key)
    {
        string fullKey = GetFullKey<T>(key);

        await _distributedCache.RemoveAsync(fullKey);

        _logger.LogTrace("Cache remove '{FullKey}'", fullKey);
    }

    private async Task SetInternalAsync<T>(string key, T obj, DistributedCacheEntryOptions? options = null)
    {
        string fullKey = GetFullKey<T>(key);

        if (obj == null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        string json = JsonSerializer.Serialize(obj);

        if (options == null)
        {
            await _distributedCache.SetStringAsync(fullKey, json);
        }
        else
        {
            await _distributedCache.SetStringAsync(fullKey, json, options);
        }

        _logger.LogTrace("Cache set '{FullKey}'", fullKey);
    }

    private static string GetFullKey<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentOutOfRangeException(nameof(key));
        }

        // The full key is the combination of type name plus unique id
        return $"{typeof(T).FullName}::{key}";
    }
}
