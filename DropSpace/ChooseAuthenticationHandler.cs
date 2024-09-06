using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace DropSpace
{
    public class ChooseAuthenticationHandler(IOptionsMonitor<CookieAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, IConfiguration configuration, IServiceProvider serviceProvider) : CookieAuthenticationHandler(options, logger, encoder)
    {
        private readonly IServiceProvider serviceProvider = serviceProvider;

        protected override Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties? properties)
        {
            properties ??= new AuthenticationProperties();


            if (user.IsInRole("OneTimeUser"))
            {
                properties.ExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(configuration.GetValue<int>("OneTimeSecsDuration"));
            }

            return base.HandleSignInAsync(user, properties);
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var result = await base.HandleAuthenticateAsync();

            if (result.Succeeded)
            {
                if (result.Principal.IsInRole("PermanentUser"))
                {
                    var id = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);

                    var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

                    var user = await userManager.GetUserAsync(result.Principal);

                    if (user == null)
                    {
                        return AuthenticateResult.Fail("Пользователь не найден");
                    }
                }
            }

            return result;
        }
    }
}
