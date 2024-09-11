using DropSpace.Events.Interfaces;

namespace DropSpace.Events.Events
{
    public class SessionExpiredEvent : IEvent
    {
        public List<string> UserIds { get; set; }
    }
}
