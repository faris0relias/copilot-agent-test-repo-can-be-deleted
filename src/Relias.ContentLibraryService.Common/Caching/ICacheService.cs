using Microsoft.Extensions.Caching.Distributed;

namespace Relias.ContentLibraryService.Common.Caching
{
    /// <summary>
    /// Basic cache service
    /// </summary>
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T obj);
        Task SetAsync<T>(string key, T obj, Func<DistributedCacheEntryOptions> options);
        Task RemoveAsync<T>(string key);
    }
}
