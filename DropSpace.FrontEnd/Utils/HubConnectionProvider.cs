﻿using DropSpace.FrontEnd.Utils.ErrorHandlers;
using DropSpace.FrontEnd.Utils.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Refit;
using System.Collections.Concurrent;

namespace DropSpace.FrontEnd.Utils
{
    public class HubConnectionProvider(
        IConfiguration configuration, 
        AuthManager authManager,
        ErrorHandlerFactory errorHandlerFactory) : IHubConnectionProvider
    {
        private readonly ErrorHandlerFactory errorHandlerFactory = errorHandlerFactory;

        ConcurrentDictionary<string, Task<HubConnection>> connections = new();

        public Task<HubConnection> GetOrCreateConnection(string hubName)
        {
            return connections.GetOrAdd(hubName, _ =>
            {
                return CreateConnection(hubName);
            });
        }

        private async Task<HubConnection> CreateConnection(string hubName)
        {
            var url = configuration.GetRequiredSection("Hubs").GetValue<string>(hubName)!;

            var connection = new HubConnectionBuilder()
                .WithUrl(url, options =>
                {
                    options.AccessTokenProvider = async () =>
                    {
                        try
                        {
                            await authManager.RefreshAccess();
                            return await authManager.GetToken();
                        } catch (ApiException)
                        {
                            await errorHandlerFactory.NotAuthorized.HandleAsync();

                            throw;
                        }
                    };
                })
                .WithAutomaticReconnect()
                .Build();

            await connection.StartAsync();

            return connection;
        }
    }
}
