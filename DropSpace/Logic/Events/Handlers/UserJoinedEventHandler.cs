using DropSpace.Infrastructure.Stores.Interfaces;
using DropSpace.Logic.Events.Events;
using DropSpace.Logic.Events.Interfaces;
using DropSpace.WebApi.SignalRHubs;
using Microsoft.AspNetCore.SignalR;

namespace DropSpace.Logic.Events.Handlers
{
    public class UserJoinedEventHandler(
        IHubContext<SessionsHub> hubContext,
        IConnectionIdStore connectionIdStore) : IEventHandler<UserJoinedEvent>
    {
        public async Task Handle(UserJoinedEvent ev)
        {
            await hubContext.Clients.Clients(
                    await connectionIdStore.GetConnectionsId(
                        ev.Session.Members
                        .Where(m => m.UserId != ev.UserId)
                        .Select(m => m.UserId)
                        .ToList()
                    )
                ).SendAsync("UserJoined", ev.Session.Members.Count);
        }
    }
}
