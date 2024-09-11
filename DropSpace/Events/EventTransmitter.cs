using DropSpace.Events.Interfaces;

namespace DropSpace.Events
{
    public class EventTransmitter(IServiceProvider serviceProvider) : IEventTransmitter
    {
        public async Task FireEvent(IEvent ev)
        {

            var eventType = ev.GetType();

            var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);

            var handler = serviceProvider.CreateScope().ServiceProvider.GetRequiredService(handlerType);

            var task = (Task)handlerType.GetMethod("Handle")!.Invoke(handler, [ev]);

            await task;
        }
    }
}
