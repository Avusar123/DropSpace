using DropSpace.Models.Data;
using DropSpace.Models.DTOs;
using DropSpace.Providers;
using DropSpace.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DropSpace.SignalRHubs
{

    [Authorize]
    public class SessionsHub(SessionService sessionService, 
        IInviteCodeProvider inviteCodeProvider,
        IConnectionIdProvider connectionIdProvider) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            
            if (Context.UserIdentifier == null) 
            {
                Context.Abort();

                return;
            }

            await connectionIdProvider.SaveConnectionId(Context.UserIdentifier, Context.ConnectionId);
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            inviteCodeProvider.RemoveUserId(Context.UserIdentifier!);

            connectionIdProvider.Remove(Context.UserIdentifier!);

            return Task.CompletedTask;
        }

        public Task<List<SessionDto>> GetSessions()
        {
            return Task.FromResult(sessionService.GetAllSessions(Context.UserIdentifier!));
        }

        public async Task<string> RefreshCode()
        {
            return await inviteCodeProvider.RefreshCode(Context.UserIdentifier!);
        }

        public async Task<bool> SendInviteByCode(string code, string sessionId)
        {
            try
            {
                var userId = await inviteCodeProvider.GetUserIdByCodeOrNull(code.ToUpper()) ?? throw new NullReferenceException("Пользователь не найден!");

                var session = await sessionService.GetAsync(Guid.Parse(sessionId));

                await Clients.User(userId).SendAsync("NewInvite", session.Name, session.Id);

                return true;
            } catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ErrorRecieved", ex.Message);

                return false;
            }
        }

        public async Task<bool> SendEvent(string sessionId, string eventName)
        {
            try
            {
                var session = await sessionService.GetAsync(Guid.Parse(sessionId));

                var connIds = await connectionIdProvider.GetConnectionsId(session.Members.Select(m => m.UserId).ToList());

                await Clients.Clients(connIds).SendAsync(eventName);

                return true;
            } catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ErrorRecieved", ex.Message);

                return false;
            }
        }
    }
}
