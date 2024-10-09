using DropSpace.Contracts.Dtos;
using DropSpace.Logic.Events.Interfaces;

namespace DropSpace.Logic.Events.Events
{
    public record FileUpdatedEvent(List<string> UserIds, FileModelDto File) : IEvent;
}
