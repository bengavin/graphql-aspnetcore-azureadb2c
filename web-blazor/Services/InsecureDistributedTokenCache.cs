using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace StarWars.UI.Blazor.Services
{
    public static class InsecureDistributedTokenCacheExtensions
    {
        public static IServiceCollection AddInsecureDistributedTokenCache(this IServiceCollection services)
        {
            services.AddSingleton<IDistributedCache, InsecureDistributedTokenCache>();
            return services;
        }
    }

    /// <summary>
    /// This cache is used to handle Blazor live-compile updates
    /// constantly losing the MSAL server-side token, ending up
    /// with a user being 'logged in', but being unable to actually
    /// obtain an access token for the user.
    ///
    /// NOTE: DO NOT USE THIS CODE OUTSIDE OF LOCAL DEVELOPMENT, EVER
    /// </summary>
    public class InsecureDistributedTokenCache : IDistributedCache
    {
        private class CacheEntry
        {
            public string Key { get; set; }
            public string Base64Value { get; set; }
            public DistributedCacheEntryOptions Options { get; set; }
            public DateTimeOffset Expiration { get; set; }
        }
        private readonly Dictionary<string, CacheEntry> _cache = new Dictionary<string, CacheEntry>();

        public InsecureDistributedTokenCache()
        {
            // read the data from disk on startup
            if (System.IO.File.Exists("App_Data/cache.json"))
            {
                _cache = JsonSerializer.Deserialize<Dictionary<string, CacheEntry>>(System.IO.File.ReadAllText("App_Data/cache.json"));
            }
        }

        private bool WriteCache()
        {
            System.IO.Directory.CreateDirectory("App_Data");
            System.IO.File.WriteAllText("App_Data/cache.json", JsonSerializer.Serialize(_cache));
            return true;
        }

        private bool FlushCache()
        {
            var cacheUpdated = false;
            foreach(var item in _cache.ToList())
            {
                if (item.Value.Expiration < DateTime.UtcNow)
                {
                    _cache.Remove(item.Key);
                    cacheUpdated = true;
                }
            }

            if (cacheUpdated) 
            {
                WriteCache();
                return true;
            }

            return false;
        }

        public byte[] Get(string key)
        {
            FlushCache();

            if (_cache.ContainsKey(key)) 
            { 
                var cacheItem = _cache[key];
                if (cacheItem.Options.SlidingExpiration.HasValue && !cacheItem.Options.AbsoluteExpiration.HasValue)
                {
                    cacheItem.Expiration = DateTime.UtcNow.Add(cacheItem.Options.SlidingExpiration.Value);
                    WriteCache();
                }

                return Convert.FromBase64String(cacheItem.Base64Value); 
            }

            return null;
        }

        public Task<byte[]> GetAsync(string key, CancellationToken cancellationToken)
        {
            return Task.FromResult(Get(key));
        }

        public void Refresh(string key) { Get(key); }
        public Task RefreshAsync(string key, CancellationToken cancellationToken) { Refresh(key); return Task.CompletedTask; }

        public void Remove(string key) 
        { 
            if (_cache.ContainsKey(key)) 
            { 
                _cache.Remove(key); 
                var dummy = FlushCache() || WriteCache(); 
            } 
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken) { Remove(key); return Task.CompletedTask; }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) 
        { 
            var newEntry = new CacheEntry
            {
                Key = key,
                Base64Value = value?.Length > 0 ? Convert.ToBase64String(value) : null,
                Options = options,
                Expiration = options.AbsoluteExpiration 
                           ?? (options.AbsoluteExpirationRelativeToNow.HasValue
                                ? DateTimeOffset.UtcNow.Add(options.AbsoluteExpirationRelativeToNow.Value)
                                : DateTimeOffset.UtcNow.Add(options.SlidingExpiration ?? TimeSpan.FromDays(1)))
            };

            if (newEntry.Base64Value != null) 
            {
                _cache[key] = newEntry;
            }
            else
            {
                Remove(key);
            }

            var dummy = FlushCache() || WriteCache();
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken cancellationToken) 
        {
            Set(key, value, options);
            return Task.CompletedTask; 
        }
    }
}