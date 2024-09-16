using DropSpace.Events.Interfaces;

namespace DropSpace.Events.Events
{
    public class FileListChangedEvent : IEvent
    {
        public List<string> UserIds { get; set; }
    }
}
