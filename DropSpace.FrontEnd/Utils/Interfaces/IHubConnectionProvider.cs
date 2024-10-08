using Microsoft.AspNetCore.SignalR.Client;

namespace DropSpace.FrontEnd.Utils.Interfaces
{
    public interface IHubConnectionProvider
    {
        Task<HubConnection> GetOrCreateConnection(string hubName);
    }
}
