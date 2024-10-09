using DropSpace.Domain;
using DropSpace.Logic.Events.Interfaces;

namespace DropSpace.Logic.Events.Events
{
    public record UserLeftEvent(Session Session, string UserId) : IEvent;
}
