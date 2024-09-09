using DropSpace.Models.Data;
using DropSpace.Models.DTOs;
using DropSpace.Providers;
using DropSpace.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DropSpace.SignalRHubs
{

    [Authorize]
    public class SessionsHub(SessionService sessionService, IInviteCodeProvider inviteCodeProvider) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            if (Context.UserIdentifier == null) 
            {
                Context.Abort();

                return;
            }
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            inviteCodeProvider.RemoveUserId(Context.UserIdentifier!);

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

        public async Task SendInviteByCode(string code, string sessionId)
        {
            try
            {
                var userId = await inviteCodeProvider.GetUserIdByCodeOrNull(code) ?? throw new NullReferenceException("Пользователь не найден!");

                var session = await sessionService.GetAsync(Guid.Parse(sessionId));

                await Clients.User(userId).SendAsync("NewInvite", session.Name, session.Id);
            } catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ErrorRecieved", ex.Message);
            }
        }
    }
}
