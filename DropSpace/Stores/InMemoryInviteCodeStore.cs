
using DropSpace.Stores.Interfaces;

namespace DropSpace.Providers
{
    public class InMemoryInviteCodeStore : IInviteCodeStore
    {
        private Dictionary<string, string> Codes = [];

        private Dictionary<string, DateTime> History = [];

        public Task<string?> GetUserIdByCodeOrNull(string code)
        {
            if (!Codes.ContainsValue(code))
            {
                return Task.FromResult<string?>(null);
            }

            if (History.TryGetValue(code, out DateTime value) && DateTime.Now - value < TimeSpan.FromMinutes(5))
            {
                throw new ArgumentException("По этому коду уже отправлено приглашение, подождите пару минут перед повторной отправкой!");
            }
            else
            {
                History[code] = DateTime.Now;
            }

            return Task.FromResult<string?>(Codes.Keys.First(key => Codes[key] == code));
        }

        public Task<string> RefreshCode(string userId)
        {
            Codes[userId] = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 6).ToUpper();

            return Task.FromResult(Codes[userId]);
        }

        public Task RemoveUserId(string userId)
        {
            Codes.Remove(userId);

            return Task.CompletedTask;
        }
    }
}
