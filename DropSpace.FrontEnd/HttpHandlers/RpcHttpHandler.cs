using System.Net.Http.Headers;
using DropSpace.FrontEnd.Utils;
using DropSpace.FrontEnd.Utils.ErrorHandlers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Refit;

namespace DropSpace.FrontEnd.HttpHandlers
{
    public class RpcHttpHandler(
        AuthManager tokenProvider, 
        ErrorHandlerFactory errorHandlerFactory,
        IConfiguration  configuration) : TokenHttpHandler(tokenProvider, errorHandlerFactory)
    {
        protected override Task ConfigureRequest(HttpRequestMessage request)
        {
            if (request.RequestUri == null)
            {
                throw new NullReferenceException("Запрос пуст!");
            }

            var baseAddress = request.RequestUri.GetLeftPart(UriPartial.Authority);

            var uriBuilder = new UriBuilder(baseAddress);

            uriBuilder.Path = configuration["rpcPrefix"] + request.RequestUri.AbsolutePath;

            request.RequestUri = uriBuilder.Uri;

            return base.ConfigureRequest(request);
        }
    }
}
