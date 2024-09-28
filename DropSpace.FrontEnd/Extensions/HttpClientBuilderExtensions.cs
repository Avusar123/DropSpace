namespace DropSpace.FrontEnd.Extensions
{
    public static class HttpClientBuilderExtensions
    {
        public static IHttpClientBuilder WithConfiguration(this IHttpClientBuilder builder, IConfiguration configuration)
        {
            builder.ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(configuration.GetValue<string>("ServerAddress")!);
            });

            return builder;
        }
    }
}
