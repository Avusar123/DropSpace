using DropSpace.Logic.Events.Interfaces;

namespace DropSpace.Logic.Events.Events
{
    public record class FileDeletedEvent(List<string> UserIds, Guid FileId) : IEvent;
}
