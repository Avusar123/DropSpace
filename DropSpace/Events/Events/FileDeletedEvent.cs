using DropSpace.Contracts.Dtos;
using DropSpace.Events.Interfaces;

namespace DropSpace.Events.Events
{
    public record class FileDeletedEvent(List<string> UserIds, Guid FileId) : IEvent;
}
