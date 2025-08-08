using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Relias.ContentLibraryService.Common.Caching
{
    /// <summary>
    /// Cache dependency injection
    /// </summary>
    public static class ConfigureCache
    {
        public static void AddCache(this IServiceCollection services, IConfiguration configuration)
        {
            CacheOptions? cacheOptions = configuration.GetSection("Cache")?.Get<CacheOptions>();

            CacheType cacheType = string.IsNullOrWhiteSpace(cacheOptions?.CacheType) ? CacheType.Memory : Enum.Parse<CacheType>(cacheOptions?.CacheType!, true);
            switch (cacheType)
            {
                case CacheType.Memory:
                    {
                        services.AddDistributedMemoryCache();
                        break;
                    }

                case CacheType.Redis:
                    {
                        // Connection string stored in Azure App Configuration
                        string connectionString = configuration["content-library-service:Cache:ConnectionString"] ?? throw new KeyNotFoundException("content-library-service:Cache:ConnectionString");
                        services.AddStackExchangeRedisCache(options => { options.Configuration = connectionString; });
                        break;
                    }

                default:
                    {
                        throw new NotSupportedException($"Cache type {cacheType} not supported");
                    }
            }

            services.AddSingleton<ICacheService, CacheService>();
        }
    }
}
