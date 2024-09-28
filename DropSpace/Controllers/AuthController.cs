using DropSpace.Contracts.Models;
using DropSpace.DataManagers;
using DropSpace.Models.Data;
using DropSpace.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Configuration;
using System.Security.Claims;
namespace DropSpace.Controllers
{

    [ApiController]
    [Route("Auth")]
    public class AuthController(JWTFactory JWTFactory,
        IConfiguration configuration,
        UserManager<IdentityUser> userManager,
        IDataProtectionProvider dataProtectionProvider) : ControllerBase
    {
        const string INVALIDLOGGINGATTEMPTMESSAGE = "Неудачная попытка входа";

        private IDataProtector dataProtector = dataProtectionProvider.CreateProtector("refresh");


        [HttpPost("OneTime")]
        public async Task<ActionResult<string>> OneTimeRegister()
        {
            var tokens = await JWTFactory.CreateTokenPair();

            SetRefreshToken(tokens.RefreshToken, tokens.Expires);

            return tokens.AccessToken;
        }

        [HttpPost("Register")]
        public async Task<ActionResult> Register(RegisterModel model)
        {

            var identityuser = new IdentityUser() { Email = model.Email, UserName = model.Email };

            var result = await userManager.CreateAsync(identityuser, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return BadRequest(ModelState);
            }

            await userManager.AddToRoleAsync(identityuser, "PermanentUser");

            return Ok();
        }



        [HttpPost]
        public async Task<ActionResult<string>> Login(LoginModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);

            string roleName = configuration.GetValue<string>("LoginUserRole")!;

            if (user == null || !await userManager.CheckPasswordAsync(user, model.Password))
            {
                ModelState.AddModelError(string.Empty, INVALIDLOGGINGATTEMPTMESSAGE);

                return BadRequest(ModelState);
            }

            var tokens = await JWTFactory.CreateTokenPair(user, roleName);

            SetRefreshToken(tokens.RefreshToken, tokens.Expires);

            return tokens.AccessToken;
        }

        [Authorize("refresh")]
        [HttpGet]
        public async Task<ActionResult<string>> Refresh()
        {
            var claims = User.Claims.ToList();

            claims.Remove(claims.First(c => c.Type == "type"));

            return await JWTFactory.CreateTokenFromClaims(claims);
        }

        [Authorize("refresh")]
        [HttpDelete]
        public async Task<ActionResult> Delete()
        {
            var cookieOptions = new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            };

            Response.Cookies.Delete("refreshToken", cookieOptions);

            return Ok();
        }

        private void SetRefreshToken(string token, DateTime expires)
        {
            var cookieOptions = new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                Expires = expires,
                SameSite = SameSiteMode.None
            };

            Response.Cookies.Append("refreshToken", dataProtector.Protect(token), cookieOptions);
        }
    }
}
