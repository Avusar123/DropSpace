using DropSpace.Events.Events;
using DropSpace.Events.Interfaces;
using DropSpace.Models.DTOs;
using DropSpace.Providers;
using DropSpace.Services;
using DropSpace.SignalRHubs;
using Microsoft.AspNetCore.SignalR;

namespace DropSpace.Events.Handlers
{
    public class SessionExpiredEventHandler(
        IHubContext<SessionsHub> hubContext,
        IConnectionIdProvider connectionIdProvider) : IEventHandler<SessionExpiredEvent>
    {
        public async Task Handle(SessionExpiredEvent ev)
        {

            await hubContext.Clients.Clients(
                    await connectionIdProvider.GetConnectionsId(
                        ev.UserIds
                    )
            ).SendAsync("SessionExpired");
        }
    }
}
