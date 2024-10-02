using DropSpace.Contracts.Dtos;
using DropSpace.Events.Interfaces;

namespace DropSpace.Events.Events
{
    public record FileUpdatedEvent(List<string> UserIds, FileModelDto File) : IEvent;
}
