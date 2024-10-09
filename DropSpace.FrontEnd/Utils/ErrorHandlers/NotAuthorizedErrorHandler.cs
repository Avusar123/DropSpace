using Microsoft.AspNetCore.Components;
using Refit;

namespace DropSpace.FrontEnd.Utils.ErrorHandlers
{
    public class NotAuthorizedErrorHandler(NavigationManager navigationManager) : ErrorHandler
    {
        public override void Handle()
        {
            var url = navigationManager.Uri;

            var uri = new Uri(url);
            if (uri.LocalPath == "/")
            {
                navigationManager.NavigateTo("/login", true);
            }
            else
            {
                navigationManager.NavigateTo($"/login?returnUrl={uri.LocalPath}", true);
            }
        }
    }
}
