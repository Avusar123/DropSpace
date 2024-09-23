using DropSpace.Models.Data;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace DropSpace.DataManagers
{
    public class ClaimsFactory(IConfiguration configuration, RoleManager<UserPlanRole> roleManager)
    {
        public async Task<ClaimsIdentity> CreateOneTimeIdentity(string authscheme)
        {

            var principleDuration = configuration.GetValue<int>("OneTimeSecsDuration");

            List<Claim> claims = [
                    new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new(ClaimTypes.Role, configuration.GetValue<string>("OneTimeRoleName")!),
                    new("expired", DateTime.Now.AddSeconds(principleDuration).Ticks.ToString())
                ];

            claims = claims.Concat(
                await GenerateRoleAdditionalClaims(configuration.GetValue<string>("OneTimeRoleName")!
                )).ToList();

            return new ClaimsIdentity(claims, authscheme);
        }

        public async Task<List<Claim>> GenerateRoleAdditionalClaims(string role)
        {
            var userPlanRole = await roleManager.FindByNameAsync(role)
                ?? throw new NullReferenceException("Роль не найдена!");

            List<Claim> claims = [
                    new("maxSessions", userPlanRole.MaxSessions.ToString()),
                    new("sessionDuration", userPlanRole.SessionDuration.ToString()),
                    new("maxFilesSize", userPlanRole.MaxSize.ToString())
                ];



            return claims;
        }
    }
}
