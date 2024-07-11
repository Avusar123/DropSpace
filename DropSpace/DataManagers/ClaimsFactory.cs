using DropSpace.Models.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DropSpace.DataManagers
{
    public class ClaimsFactory(IConfiguration configuration)
    {
        public ClaimsIdentity CreateOneTimeIdentity(string authscheme)
        {

            var principleDuration = configuration.GetValue<int>("OneTimeSecsDuration");

            List<Claim> claims = [
                    new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new("status", "OneTimeUser"),
                    new("expired", DateTime.Now.AddSeconds(principleDuration).Ticks.ToString())
                ];

            return new ClaimsIdentity(claims, authscheme);
        }
    }
}
