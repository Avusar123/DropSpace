using DropSpace.Utils.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using NuGet.Common;

namespace DropSpace.Utils
{
    public class SeparetedCashe<T>(IDistributedCache distributedCache) : ISeparetedCashe<T>
    {
        public byte[]? Get(string key)
        {
            return distributedCache.Get(GetKey(key));
        }

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        {
            return distributedCache.GetAsync(GetKey(key), token);
        }

        public void Refresh(string key)
        {
            distributedCache.Refresh(GetKey(key));
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            return distributedCache.RefreshAsync(GetKey(key), token);
        }

        public void Remove(string key)
        {
            distributedCache.Remove(GetKey(key));
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            return distributedCache.RemoveAsync(GetKey(key), token);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            distributedCache.Set(GetKey(key), value, options);
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            return distributedCache.SetAsync(GetKey(key), value, options, token);
        }

        private static string GetKey(string key) => typeof(T).FullName + "_" + key;
    }
}
