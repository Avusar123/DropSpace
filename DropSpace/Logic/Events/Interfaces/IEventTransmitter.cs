namespace DropSpace.Logic.Events.Interfaces
{

    public interface IEventTransmitter
    {
        public Task FireEvent(IEvent ev);
    }
}
