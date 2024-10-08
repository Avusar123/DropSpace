using DropSpace.Contracts.Dtos;
using DropSpace.Extensions;
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
        IConnectionIdStore connectionIdStore,
        ILogger<SessionsHub> logger) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            logger.LogDebug("Клиент {UserIdentifier} с {ConnectionId} присоединился к SessionsHub",
                Context.UserIdentifier, Context.ConnectionId);


            if (Context.UserIdentifier == null)
            {
                Context.Abort();

                return;
            }

            await connectionIdStore.SaveConnectionId(Context.UserIdentifier, Context.ConnectionId);
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            logger.LogDebug("Клиент {UserIdentifier} с {ConnectionId} вышел из SessionsHub",
                Context.UserIdentifier, Context.ConnectionId);

            inviteCodeStore.RemoveUserId(Context.UserIdentifier!);

            connectionIdStore.Remove(Context.UserIdentifier!);

            return Task.CompletedTask;
        }

        public async Task<string> GetInviteCode()
        {
            return await inviteCodeStore.RefreshCode(Context.UserIdentifier!);
        }

        public async Task SendInviteByCode(string code, Guid sessionId)
        {
            var userId = await inviteCodeStore.GetUserIdByCodeOrNull(code.ToUpper())
                ?? throw new NullReferenceException("Пользователь не найден!");

            var session = await sessionService.GetAsync(sessionId);

            await Clients.User(userId).SendAsync("NewInvite", session.ToDto());
        }
    }
}
