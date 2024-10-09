using DropSpace.Infrastructure.Stores.Interfaces;
using DropSpace.WebApi.Utils.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace DropSpace.Infrastructure.Stores
{
    public class CasheInviteCodeStore(ISeparetedCashe<CasheInviteCodeStore> cashe) : IInviteCodeStore
    {
        public async Task<string?> GetUserIdByCodeOrNull(string code)
        {
            var history = await cashe.GetAsync("history_" + code);

            if (history != null)
            {
                throw new ArgumentException("По этому коду уже отправлено приглашение, подождите пару минут перед повторной отправкой!");
            }

            var userId = await cashe.GetStringAsync(code);

            await cashe.SetStringAsync("history_" + code, string.Empty, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            });

            return userId;
        }

        public async Task<string> RefreshCode(string userId)
        {
            var code = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 5).ToUpper();

            await cashe.SetStringAsync(code, userId);

            return code;
        }

        public async Task RemoveUserId(string userId)
        {
            await cashe.RemoveAsync(userId);
        }
    }
}
