using DropSpace.Infrastructure.Stores.Interfaces;
using DropSpace.Logic.Events.Events;
using DropSpace.Logic.Events.Interfaces;
using DropSpace.WebApi.SignalRHubs;
using Microsoft.AspNetCore.SignalR;

namespace DropSpace.Logic.Events.Handlers
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
