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
        IConnectionIdStore connectionIdStore
        ) : IEventHandler<SessionExpiredEvent>
    {
        public async Task Handle(SessionExpiredEvent ev)
        {

            await hubContext.Clients.Clients(
                    await connectionIdStore.GetConnectionsId(
                        ev.UserIds
                    )
            ).SendAsync("SessionExpired");
        }
    }
}
