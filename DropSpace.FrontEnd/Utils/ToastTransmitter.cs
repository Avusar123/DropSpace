using BlazorBootstrap;

namespace DropSpace.FrontEnd.Utils
{
    public delegate void ShowToast(ToastMessage toastMessage);

    public class ToastTransmitter
    {
        public event ShowToast OnShowToast;

        public void ShowToast(ToastMessage toastMessage)
        {
            OnShowToast.Invoke(toastMessage);
        }
    }
}
