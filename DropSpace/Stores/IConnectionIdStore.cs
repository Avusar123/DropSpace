namespace DropSpace.Providers
{
    public interface IConnectionIdStore
    {
        public Task SaveConnectionId(string userId, string connectionId);

        public Task Remove(string connectionId);

        public Task<List<string>> GetConnectionsId(List<string> userIds);
    }
}
