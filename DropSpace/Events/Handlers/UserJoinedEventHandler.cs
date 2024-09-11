using DropSpace.Events.Events;
using DropSpace.Events.Interfaces;
using DropSpace.Models.Data;
using DropSpace.Models.DTOs;
using DropSpace.Providers;
using DropSpace.Services;
using DropSpace.SignalRHubs;
using Microsoft.AspNetCore.SignalR;

namespace DropSpace.Events.Handlers
{
    public class UserJoinedEventHandler(
        IHubContext<SessionsHub> hubContext, 
        IConnectionIdStore connectionIdStore) : IEventHandler<UserJoinedEvent>
    {
        public async Task Handle(UserJoinedEvent ev)
        {
            await hubContext.Clients.Clients(
                    await connectionIdStore.GetConnectionsId(
                        ev.Session
                        .Members
                        .Where(m => m.UserId != ev.UserId)
                        .Select(m => m.UserId).ToList()
                    )
                ).SendAsync("NewUser", new SessionDto(ev.Session.Id, ev.Session.Name, ev.Session.Members.Count));
        }
    }
}
