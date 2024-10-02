using Microsoft.Extensions.Caching.Distributed;

namespace DropSpace.Utils.Interfaces
{
    public interface ISeparetedCashe<T> : IDistributedCache
    {
    }
}
