using DropSpace.Contracts.Models;
using DropSpace.WebApi.Controllers.Filters.MemberFilter.Providers;
using Uploads;

namespace DropSpace.WebApi.Controllers.Filters.MemberFilter
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection RegisterSessionIdProviders(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ISessionIdProvider<DeleteFileModel>, DeleteFileModelSessionIdProvider>();

            return serviceCollection;
        }
    }
}
