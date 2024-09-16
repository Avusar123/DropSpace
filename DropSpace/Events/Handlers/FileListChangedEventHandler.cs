using DropSpace.Events.Events;
using DropSpace.Events.Interfaces;
using DropSpace.SignalRHubs;
using DropSpace.Stores.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace DropSpace.Events.Handlers
{
    public class FileListChangedEventHandler(
        IHubContext<SessionsHub> hubContext,
        IConnectionIdStore connectionIdStore
        ) : IEventHandler<FileListChangedEvent>
    {
        public async Task Handle(FileListChangedEvent ev)
        {
            await hubContext.Clients.Clients(
                    await connectionIdStore.GetConnectionsId(
                        ev.UserIds
                    )
            ).SendAsync("FileListChanged");
        }
    }
}
