using DropSpace.Contracts.Dtos;
using DropSpace.Services.Interfaces;
using DropSpace.Stores.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DropSpace.SignalRHubs
{

    [Authorize]
    public class SessionsHub(
        ISessionService sessionService,
        IInviteCodeStore inviteCodeStore,
        IConnectionIdStore connectionIdStore) : Hub
    {
        public override async Task OnConnectedAsync()
        {

            if (Context.UserIdentifier == null)
            {
                Context.Abort();

                return;
            }

            await connectionIdStore.SaveConnectionId(Context.UserIdentifier, Context.ConnectionId);
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            inviteCodeStore.RemoveUserId(Context.UserIdentifier!);

            connectionIdStore.Remove(Context.UserIdentifier!);

            return Task.CompletedTask;
        }

        public async Task<string> GetInviteCode()
        {
            return await inviteCodeStore.RefreshCode(Context.UserIdentifier!);
        }

        public async Task<bool> SendInviteByCode(string code, string sessionId)
        {
            try
            {
                var userId = await inviteCodeStore.GetUserIdByCodeOrNull(code.ToUpper())
                    ?? throw new NullReferenceException("Пользователь не найден!");

                var session = await sessionService.GetAsync(Guid.Parse(sessionId));

                await Clients.User(userId).SendAsync("NewInvite", session.Name, session.Id);

                return true;
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ErrorRecieved", ex.Message);

                return false;
            }
        }
    }
}
