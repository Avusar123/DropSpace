using DropSpace.Events.Events;
using DropSpace.Events.Interfaces;
using DropSpace.Models.DTOs;
using DropSpace.Providers;
using DropSpace.Services;
using DropSpace.SignalRHubs;
using Microsoft.AspNetCore.SignalR;

namespace DropSpace.Events.Handlers
{
    public class UserLeftEventHandler(
        IHubContext<SessionsHub> hubContext,
        IConnectionIdProvider connectionIdProvider) : IEventHandler<UserLeftEvent>
    {
        public async Task Handle(UserLeftEvent ev)
        {

            await hubContext.Clients.Clients(
                    await connectionIdProvider.GetConnectionsId(
                        ev.Session.Members
                        .Where(m => m.UserId != ev.UserId)
                        .Select(m => m.UserId).ToList()
                    )
                ).SendAsync("UserLeft", new SessionDto(ev.Session.Id, ev.Session.Name, ev.Session.Members.Count));
        }
    }
}
