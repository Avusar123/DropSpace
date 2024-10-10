using DropSpace.Infrastructure.Stores.Interfaces;
using DropSpace.Logic.Events.Events;
using DropSpace.Logic.Events.Interfaces;
using DropSpace.WebApi.SignalRHubs;
using Microsoft.AspNetCore.SignalR;

namespace DropSpace.Logic.Events.Handlers
{
    public class FileUpdatedEventHandler(
        IHubContext<SessionsHub> hubContext,
        IConnectionIdStore connectionIdStore
        ) : IEventHandler<FileUpdatedEvent>
    {
        public async Task Handle(FileUpdatedEvent ev)
        {
            await hubContext.Clients.Clients(
                    await connectionIdStore.GetConnectionsId(
                        ev.UserIds
                    )
            ).SendAsync("FileUpdated", ev.File);
        }
    }
}
