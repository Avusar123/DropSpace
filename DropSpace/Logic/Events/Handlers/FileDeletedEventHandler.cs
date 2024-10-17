using DropSpace.Infrastructure.Stores.Interfaces;
using DropSpace.Logic.Events.Events;
using DropSpace.Logic.Events.Interfaces;
using DropSpace.WebApi.SignalRHubs;
using Microsoft.AspNetCore.SignalR;

namespace DropSpace.Logic.Events.Handlers
{
    public class FileDeletedEventHandler(
            IHubContext<SessionsHub> hubContext,
            IConnectionIdStore connectionIdStore) : IEventHandler<FileDeletedEvent>
    {
        public async Task Handle(FileDeletedEvent ev)
        {
            await hubContext.Clients.Clients(
                    await connectionIdStore.GetConnectionsId(
                        ev.UserIds
                    )
            ).SendAsync("FileDeleted", ev.FileId);
        }
    }
}
