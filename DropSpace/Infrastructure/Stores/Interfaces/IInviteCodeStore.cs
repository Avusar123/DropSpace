namespace DropSpace.Infrastructure.Stores.Interfaces
{
    public interface IInviteCodeStore
    {
        public Task<string> RefreshCode(string userId);

        public Task<string?> GetUserIdByCodeOrNull(string code);

        public Task RemoveUserId(string userId);
    }
}
