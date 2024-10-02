using DropSpace.Events.Interfaces;
using DropSpace.Models.Data;

namespace DropSpace.Events.Events
{
    public record SessionExpiredEvent(Session Session) : IEvent;
}
