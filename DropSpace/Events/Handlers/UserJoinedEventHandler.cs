using DropSpace.Contracts.Dtos;
using DropSpace.Events.Events;
using DropSpace.Events.Interfaces;
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
                        ev.Session
                        .Members
                        .Where(m => m.UserId != ev.UserId)
                        .Select(m => m.UserId).ToList()
                    )
                ).SendAsync("NewUser", new SessionDto(ev.Session.Id, ev.Session.Name, ev.Session.Members.Count));
        }
    }
}
