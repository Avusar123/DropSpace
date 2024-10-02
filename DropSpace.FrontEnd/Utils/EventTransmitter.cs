using BlazorBootstrap;

namespace DropSpace.FrontEnd.Utils
{

    public class EventTransmitter
    {
        Dictionary<string, Func<object, Task>> handlers = [];

        public void On<T>(string key,  Func<T, Task> handler)
        {
            if (handlers.ContainsKey(key))
            {
                handlers[key] += async (obj) => await handler((T)obj);
            }
            else
            {
                handlers[key] = async (obj) => await handler((T)obj);
            }
        }

        public async Task InvokeAsync<T>(string key, T args)
        {
            await handlers[key].Invoke(args!);
        }
    }
}
