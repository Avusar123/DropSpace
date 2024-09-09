namespace DropSpace.Providers
{
    public interface IInviteCodeProvider
    {
        public Task<string> RefreshCode(string userId);

        public Task<string?> GetUserIdByCodeOrNull(string code);

        public Task RemoveUserId(string userId);
    }
}
