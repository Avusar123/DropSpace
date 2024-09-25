
using DropSpace.Stores.Interfaces;

namespace DropSpace.Providers
{
    public class InMemoryConnectionIdStore : IConnectionIdStore
    {
        Dictionary<string, string> connectionIds = [];

        public Task<List<string>> GetConnectionsId(List<string> userIds)
        {
            List<string> result = [];

            foreach (var userId in userIds)
            {

                if (connectionIds.TryGetValue(userId, out var connId))
                {
                    result.Add(connId);
                }
            }

            return Task.FromResult(result);
        }

        public Task Remove(string connectionId)
        {
            var pair = connectionIds.FirstOrDefault(pair => pair.Value == connectionId);

            if (pair.Value == null || pair.Key == null || !connectionIds.ContainsKey(pair.Key))
                return Task.CompletedTask;

            connectionIds.Remove(connectionIds[pair.Key]);


            return Task.CompletedTask;
        }

        public Task SaveConnectionId(string userId, string connectionId)
        {
            connectionIds[userId] = connectionId;

            return Task.CompletedTask;
        }
    }
}
