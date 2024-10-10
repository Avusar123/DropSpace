using BlazorBootstrap;

namespace DropSpace.FrontEnd.Utils.ErrorHandlers
{
    public class MessageErrorHandler(EventTransmitter eventTransmitter) : ErrorHandler<string>
    {
        public async override Task HandleAsync(string arg)
        {
            await eventTransmitter.InvokeAsync<ToastMessage>("ShowToast",
                new()
                {
                    AutoHide = true,
                    Type = ToastType.Danger,
                    Message = arg
                });
        }
    }
}
