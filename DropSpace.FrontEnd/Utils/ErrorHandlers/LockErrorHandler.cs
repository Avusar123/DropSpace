
using BlazorBootstrap;

namespace DropSpace.FrontEnd.Utils.ErrorHandlers
{
    public class LockErrorHandler(EventTransmitter eventTransmitter) : ErrorHandler
    {
        public async override Task HandleAsync()
        {
            await eventTransmitter.InvokeAsync<ToastMessage>("ShowToast",
                new()
                {
                    AutoHide = true,
                    Type = ToastType.Danger,
                    Message = "Подождите, действие уже выполняется.."
                });
        }
    }
}
