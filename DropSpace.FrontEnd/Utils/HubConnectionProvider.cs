using DropSpace.FrontEnd.Utils.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Concurrent;

namespace DropSpace.FrontEnd.Utils
{
    public class HubConnectionProvider(IConfiguration configuration, AuthManager authManager) : IHubConnectionProvider
    {
        ConcurrentDictionary<string, HubConnection> connections = new();

        public async Task<HubConnection> GetOrCreateConnection(string hubName)
        {
            if (connections.ContainsKey(hubName)
                && connections[hubName].State != HubConnectionState.Disconnected)
            {
                return connections[hubName];
            }

            var url = configuration.GetRequiredSection("Hubs").GetValue<string>(hubName)!;

            var connection = new HubConnectionBuilder()
                .WithUrl(url, options =>
                {
                    options.AccessTokenProvider = async () =>
                    {
                        await authManager.RefreshAccess();
                        return await authManager.GetToken();
                    };
                })
                .WithAutomaticReconnect()
                .Build();

            await connection.StartAsync();

            connections[hubName] = connection;

            return connection;
        }
    }
}
