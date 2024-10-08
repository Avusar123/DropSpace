using DropSpace.Contracts.Dtos;
using DropSpace.Events.Events;
using DropSpace.Events.Interfaces;
using DropSpace.Extensions;
using DropSpace.SignalRHubs;
using DropSpace.Stores.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace DropSpace.Events.Handlers
{
    public class UserLeftEventHandler(
        IHubContext<SessionsHub> hubContext,
        IConnectionIdStore connectionIdStore) : IEventHandler<UserLeftEvent>
    {
        public async Task Handle(UserLeftEvent ev)
        {
            await hubContext.Clients.Clients(
                    await connectionIdStore.GetConnectionsId(
                        ev.Session.Members
                        .Where(m => m.UserId != ev.UserId)
                        .Select(m => m.UserId)
                        .ToList()
                    )
                ).SendAsync("UserLeft", ev.Session.Members.Count);
        }
    }
}
