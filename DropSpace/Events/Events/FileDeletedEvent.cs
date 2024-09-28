using DropSpace.Contracts.Dtos;
using DropSpace.Events.Interfaces;

namespace DropSpace.Events.Events
{
    public class FileDeletedEvent : IEvent
    {
        public List<string> UserIds { get; set; }

        public PendingUploadModelDto Upload { get; set; }
    }
}
