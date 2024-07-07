
using OpenIddict.Abstractions;
using System.Security.Cryptography;
using static OpenIddict.Abstractions.OpenIddictConstants;

public class DataSeedService(IServiceProvider serviceProvider) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();

        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        if (await manager.CountAsync() == 0)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor()
            {
                ClientId = "test",
                ClientSecret = "test",
                Permissions =
                {
                    Permissions.GrantTypes.ClientCredentials,
                    Permissions.Endpoints.Token,
                    Permissions.Prefixes.Scope + "home_api"
                },
            });

            await manager.CreateAsync(new OpenIddictApplicationDescriptor()
            {
                ClientId = "test_mvc",
                ConsentType = ConsentTypes.Explicit,
                DisplayName = "Test MVC",
                ClientType = ClientTypes.Public,
                PostLogoutRedirectUris =
                {
                    new Uri("https://localhost:7237/authentication/logout-callback")
                },
                RedirectUris =
                {
                    new Uri("https://localhost:7237/authentication/login-callback")
                },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Logout,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles
                },
                Requirements =
                {
                    Requirements.Features.ProofKeyForCodeExchange
                }
            });
        }
    }
}