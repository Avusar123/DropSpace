namespace DropSpace.Logic.Events.Interfaces
{
    public interface IEventHandler<T>
        where T : IEvent
    {
        public Task Handle(T ev);
    }
}
