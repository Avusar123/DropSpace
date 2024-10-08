using DropSpace.Stores.Interfaces;
using DropSpace.Utils.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace DropSpace.Stores
{
    public class CasheConnectionIdStore(ISeparetedCashe<CasheConnectionIdStore> cashe) : IConnectionIdStore
    {
        public async Task<List<string>> GetConnectionsId(List<string> userIds)
        {
            List<string> result = [];

            foreach (var userId in userIds)
            {
                var connId = await cashe.GetStringAsync("reversed_" + userId);

                if (connId != null)
                {
                    result.Add(connId);
                }
            }

            return result;
        }

        public async Task Remove(string connectionId)
        {
            var userId = await cashe.GetStringAsync(connectionId);

            if (userId != null)
                await cashe.RemoveAsync("reverse_" + userId);

            await cashe.RemoveAsync(connectionId);
        }

        public async Task SaveConnectionId(string userId, string connectionId)
        {
            await cashe.SetStringAsync(connectionId, userId);

            await cashe.SetStringAsync("reversed_" + userId, connectionId);
        }
    }
}
