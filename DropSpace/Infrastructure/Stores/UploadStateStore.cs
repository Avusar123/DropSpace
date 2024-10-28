using DropSpace.Domain;
using DropSpace.Infrastructure.Stores.Interfaces;
using DropSpace.WebApi.Utils.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Google.Protobuf;
using Microsoft.EntityFrameworkCore;
using Uploads;

namespace DropSpace.Infrastructure.Stores
{
    public class UploadStateStore(ISeparetedCache<UploadStateStore> cache, IConfiguration configuration) : IUploadStateStore
    {
        private readonly TimeSpan uploadExpiration = TimeSpan.FromSeconds(configuration.GetValue<int>("UploadTimeOutSecs"));

        public async Task SetAsync(UploadState model, Guid fileId)
        {
            await cache.SetAsync(fileId.ToString(), model.ToByteArray(), new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = uploadExpiration
            });
        }

        public async Task DeleteAsync(Guid fileId)
        {
            await cache.RemoveAsync(fileId.ToString());
        }

        public async Task<UploadState?> GetByFileId(Guid fileId)
        {
            var result = await cache.GetAsync(fileId.ToString());

            if (result == null)
            {
                return null;
            }

            return UploadState.Parser.ParseFrom(result);
        }
    }
}
