using DropSpace.Domain;
using DropSpace.WebApi.Utils.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DropSpace.WebApi.Utils.JWTs
{
    public record TokensResult(string AccessToken, string RefreshToken, DateTime Expires);

    public class JWTFactory(
        IConfiguration configuration,
        IRSAKeyProvider rSAKeyProvider,
        RoleManager<UserPlanRole> roleManager,
        IUserClaimsPrincipalFactory<IdentityUser> claimsPrincipalFactory)
    {
        public Task<string> CreateTokenFromClaims(List<Claim> claims, DateTime? expires = null)
        {
            expires ??= DateTime.UtcNow.AddMinutes(5);

            var handler = new JwtSecurityTokenHandler();

            var key = rSAKeyProvider.GetOrCreateKey();

            var creds = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return Task.FromResult(handler.WriteToken(token));
        }

        public async Task<TokensResult> CreateTokenPair()
        {
            string roleName = configuration.GetValue<string>("OneTimeRoleName")!;

            DateTime oneTimeExpires = DateTime.UtcNow.AddSeconds(configuration.GetValue<int>("OneTimeSecsDuration"));

            List<Claim> claims = [
                    new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new(ClaimTypes.Role, roleName),
                ];

            claims = claims
                .Concat(await GenerateRoleAdditionalClaims(roleName))
                .ToList();

            var accessToken = await CreateTokenFromClaims(claims, DateTime.UtcNow.AddMinutes(5));

            claims.Add(new("type", "refresh"));

            var refreshToken = await CreateTokenFromClaims(claims, oneTimeExpires);

            return new TokensResult(
                    accessToken,
                    refreshToken,
                    oneTimeExpires
            );
        }

        public async Task<TokensResult> CreateTokenPair(IdentityUser identityUser, string roleName)
        {
            DateTime permanentUserExpires = DateTime.UtcNow.AddSeconds(configuration.GetValue<int>("PermanentUserSecsDuration"));

            var identity = (await claimsPrincipalFactory.CreateAsync(identityUser)).Identities.First();

            identity.AddClaims(await GenerateRoleAdditionalClaims(roleName));

            var claims = identity.Claims.ToList();

            var accessToken = await CreateTokenFromClaims(claims, DateTime.UtcNow.AddMinutes(5));

            claims.Add(new("type", "refresh"));

            var refreshToken = await CreateTokenFromClaims(claims, permanentUserExpires);

            return new TokensResult(
                    accessToken,
                    refreshToken,
                    permanentUserExpires
            );
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
    }
}
