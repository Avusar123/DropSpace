using System.Net.Http.Headers;
using DropSpace.FrontEnd.Utils;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Refit;

namespace DropSpace.FrontEnd.HttpHandlers
{
    public class TokenHttpHandler(AuthManager tokenProvider) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
                var token = await tokenProvider.GetToken();

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await base.SendAsync(request, cancellationToken);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await tokenProvider.RefreshAccess();

                    return await SendAsync(request, cancellationToken);
                }

                return response;
        }
    }
}
