using DropSpace.Events.Events;
using DropSpace.Events.Interfaces;
using DropSpace.SignalRHubs;
using DropSpace.Stores.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace DropSpace.Events.Handlers
{
    public class NewChunkUploadedEventHandler(
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
