using Blazored.LocalStorage;
using DropSpace.FrontEnd.Services;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace DropSpace.FrontEnd.Utils
{
    public class AuthManager(
        IAuthService authService, 
        ILocalStorageService localStorageService)
    {
        public async Task RefreshAccess()
        {
            var token = await authService.Refresh();
            await SaveToken(token);
        }

        public async Task SaveToken(string token)
        {
            await localStorageService.SetItemAsync("accessToken", token);
        }

        public async Task<List<Claim>> GetClaims()
        {
            var token = await GetToken();

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);

            return jsonToken.Claims.ToList();
        }

        public async Task<string> GetToken()
        {
            if (!await localStorageService.ContainKeyAsync("accessToken"))
            {
                await RefreshAccess();
            }

            return (await localStorageService.GetItemAsStringAsync("accessToken"))!.Replace("\"", "");
        }
    }
}
