using Microsoft.Extensions.Caching.Distributed;

namespace DropSpace.WebApi.Utils.Interfaces
{
    public interface ISeparetedCache<T> : IDistributedCache
    {
    }
}
