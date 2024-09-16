using DropSpace.Events.Interfaces;
using DropSpace.Models.DTOs;

namespace DropSpace.Events.Events
{
    public class NewChunkUploadedEvent : IEvent
    {
        public List<string> UserIds { get; set; }

        public PendingUploadModelDto Upload { get; set; }
    }
}
