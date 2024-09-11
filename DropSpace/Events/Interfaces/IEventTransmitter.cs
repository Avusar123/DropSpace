namespace DropSpace.Events.Interfaces
{

    public interface IEventTransmitter
    {
        public Task FireEvent(IEvent ev);
    }
}
