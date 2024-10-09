using DropSpace.Infrastructure.Stores.Interfaces;
using DropSpace.Logic.Events.Events;
using DropSpace.Logic.Events.Interfaces;
using DropSpace.Logic.Extensions;
using DropSpace.WebApi.SignalRHubs;
using Microsoft.AspNetCore.SignalR;

namespace DropSpace.Logic.Events.Handlers
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
                        ev.Session.GetMemberIds()
                    )
            ).SendAsync("SessionExpired", ev.Session.Id);
        }
    }
}
