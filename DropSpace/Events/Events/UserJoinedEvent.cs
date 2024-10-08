using DropSpace.Events.Interfaces;
using DropSpace.Models.Data;

namespace DropSpace.Events.Events
{
    public record UserJoinedEvent(Session Session, string UserId) : IEvent;
}
