using DropSpace.Models.Data;
using DropSpace.RSAKeyProviders;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DropSpace.DataManagers
{
    public class JWTFactory(
        IConfiguration configuration, 
        IRSAKeyProvider rSAKeyProvider,
        RoleManager<UserPlanRole> roleManager)
    {
        public async Task<string> CreateOneTimeIdentityToken()
        {
            var principleDuration = configuration.GetValue<int>("OneTimeSecsDuration");

            string roleName = configuration.GetValue<string>("OneTimeRoleName")!;

            List<Claim> claims = [
                    new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new(ClaimTypes.Role, roleName),
                ];

            claims = claims.Concat(await GenerateRoleAdditionalClaims(roleName)).ToList();

            return GenerateToken(claims, DateTime.Now.AddSeconds(principleDuration));
        }

        public async Task<string> CreateTokenFromIdentity(IdentityUser identityUser, string roleName)
        {
            var principleDuration = configuration.GetValue<int>("PemanentUserSecsDuration");

            List<Claim> claims = [
                    new(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
                    new(ClaimTypes.Role, roleName)
                ];

            claims = claims.Concat(await GenerateRoleAdditionalClaims(roleName)).ToList();

            return GenerateToken(claims, DateTime.Now.AddSeconds(principleDuration));
        }

        private async Task<List<Claim>> GenerateRoleAdditionalClaims(string role)
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

        private string GenerateToken(List<Claim> claims, DateTime expires)
        {
            var handler = new JwtSecurityTokenHandler();

            var key = rSAKeyProvider.GetKey();

            var creds = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return handler.WriteToken(token);
        }
    }
}
