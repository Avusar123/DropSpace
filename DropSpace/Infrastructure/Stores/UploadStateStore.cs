using DropSpace.Domain;
using DropSpace.Infrastructure.Stores.Interfaces;
using DropSpace.WebApi.Utils.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Google.Protobuf;
using Microsoft.EntityFrameworkCore;
using Uploads;

namespace DropSpace.Infrastructure.Stores
{
    public class UploadStateStore(ISeparetedCache<UploadStateStore> cache) : IUploadStateStore
    {
        public async Task SetAsync(UploadState model, Guid fileId)
        {
            await cache.SetAsync(fileId.ToString(), model.ToByteArray(), new DistributedCacheEntryOptions()
            {
                //AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
            });
        }

        public async Task DeleteAsync(Guid fileId)
        {
            await cache.RemoveAsync(fileId.ToString());
        }

        public async Task<UploadState> GetByFileId(Guid fileId)
        {
            var result = await cache.GetAsync(fileId.ToString());

            return UploadState.Parser.ParseFrom(result);
        }
    }
}
