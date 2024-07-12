using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace DropSpace
{
    public class ChooseAuthenticationHandler(IOptionsMonitor<CookieAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, IConfiguration configuration) : CookieAuthenticationHandler(options, logger, encoder)
    {
        protected override Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties? properties)
        {
            properties ??= new AuthenticationProperties();


            if (user.IsInRole("OneTimeUser"))
            {
                properties.ExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(configuration.GetValue<int>("OneTimeSecsDuration"));
            }

            return base.HandleSignInAsync(user, properties);
        }
    }
}
