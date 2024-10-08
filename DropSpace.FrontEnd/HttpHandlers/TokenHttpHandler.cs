﻿using System.Net.Http.Headers;
using DropSpace.FrontEnd.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Refit;

namespace DropSpace.FrontEnd.HttpHandlers
{
    public class TokenHttpHandler(AuthManager tokenProvider, NavigationManager navigationManager) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
                var token = await tokenProvider.GetToken();

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await base.SendAsync(request, cancellationToken);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    try
                    {
                        await tokenProvider.RefreshAccess();
                    }
                    catch (ApiException ex) 
                    {
                        if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            navigationManager.NavigateTo("/login", true);
                            return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
                        }
                    }
                    

                    return await SendAsync(request, cancellationToken);
                }

                return response;
        }
    }
}
