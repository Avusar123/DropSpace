
using BlazorBootstrap;

namespace DropSpace.FrontEnd.Utils.ErrorHandlers
{
    public class HubErrorHandler(EventTransmitter eventTransmitter) : MessageErrorHandler(eventTransmitter)
    {
        public async override Task HandleAsync(string arg)
        {
            var message = arg.Split(":").Last();

            await base.HandleAsync(message);
        }
    }
}
