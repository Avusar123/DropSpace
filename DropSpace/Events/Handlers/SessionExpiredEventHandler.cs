﻿using DropSpace.Events.Events;
using DropSpace.Events.Interfaces;
using DropSpace.Extensions;
using DropSpace.SignalRHubs;
using DropSpace.Stores.Interfaces;
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
                        ev.Session.GetMemberIds()
                    )
            ).SendAsync("SessionExpired", ev.Session.Id);
        }
    }
}
