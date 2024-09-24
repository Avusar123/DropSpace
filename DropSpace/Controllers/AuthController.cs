using DropSpace.DataManagers;
using DropSpace.Models;
using DropSpace.Models.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
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
        public async Task<IActionResult> OneTimeRegister()
        {
            var token = await JWTFactory.CreateOneTimeIdentityToken();

            return Ok(token);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterModel model)
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
        public async Task<IActionResult> Login(LoginModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);

            if (user == null || !await userManager.CheckPasswordAsync(user, model.Password))
            {
                ModelState.AddModelError(string.Empty, INVALIDLOGGINGATTEMPTMESSAGE);

                return BadRequest(ModelState);
            }

            return Ok(await JWTFactory.CreateTokenFromIdentity(user, configuration.GetValue<string>("LoginUserRole")!));
        }

    }
}
