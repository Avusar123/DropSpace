using DropSpace.DataManagers;
using DropSpace.Models;
using DropSpace.Models.Data;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
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
        UserManager<IdentityUser> userManager) : ControllerBase
    {
        const string INVALIDLOGGINGATTEMPTMESSAGE = "Неудачная попытка входа";


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



        [HttpPost("Login")]
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
        [HttpPost("Refresh")]
        public ActionResult<string> Refresh()
        {
            var claims = User.Claims.ToList();

            claims.Remove(claims.First(c => c.Type == "type"));

            return JWTFactory.CreateTokenFromClaims(claims);
        }

        private void SetRefreshToken(string token, DateTime expires)
        {
            var cookieOptions = new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                Expires = expires
            };

            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }
    }
}
