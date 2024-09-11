using DropSpace.Events.Interfaces;
using DropSpace.Models.Data;

namespace DropSpace.Events.Events
{
    public class UserLeftEvent : IEvent
    {
        public Session Session { get; set; }

        public string UserId { get; set; }
    }
}
