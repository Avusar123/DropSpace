﻿using DropSpace.Contracts.Dtos;
using DropSpace.Events.Events;
using DropSpace.Events.Interfaces;
using DropSpace.Extensions;
using DropSpace.SignalRHubs;
using DropSpace.Stores.Interfaces;
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
                        ev.Session.GetMemberIds()
                    )
                ).SendAsync("UserJoined", ev.Session.ToDto());
        }
    }
}
