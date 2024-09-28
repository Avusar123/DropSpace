using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System.Net;

namespace DropSpace.FrontEnd.HttpHandlers
{
    public class CookieHttpHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            var response = await base.SendAsync(request, cancellationToken);

            return response;
        }
    }
}
