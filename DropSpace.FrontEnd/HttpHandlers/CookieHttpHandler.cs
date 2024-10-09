using DropSpace.FrontEnd.Utils.ErrorHandlers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Refit;
using System.Net;

namespace DropSpace.FrontEnd.HttpHandlers
{
    public class CookieHttpHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                ErrorHandler.NotAuthorized.Handle();

                return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            }

            return response;
        }
    }
}
