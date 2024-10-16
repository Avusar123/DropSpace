using Grpc.Net.Client.Web;

namespace DropSpace.FrontEnd.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IHttpClientBuilder AddGrpcClient<T>(this IServiceCollection builder, IConfiguration configuration)
            where T : class
        {
            return builder.AddGrpcClient<T>(configureClient: options =>
                options.Address = new Uri(configuration.GetValue<string>("gRPCServerAddress") 
                                        ?? throw new NullReferenceException("Конфигурация отсутствует!")
                )).ConfigurePrimaryHttpMessageHandler(() => new GrpcWebHandler(new HttpClientHandler()));
        }
    }
}
