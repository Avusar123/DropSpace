using DropSpace.DataManagers;
using DropSpace.Models;
using DropSpace.Models.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace DropSpace.Controllers
{
    [Route("Auth")]
    public class AuthController(ClaimsFactory claimsFactory,
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        RoleManager<UserPlanRole> roleManager) : Controller
    {
        const string INVALIDLOGGINGATTEMPTMESSAGE = "Неудачная попытка входа";


        [HttpPost("OneTime")]
        public async Task<IActionResult> OneTimeRegister(string? returnUrl)
        {

            var identity = await claimsFactory.CreateOneTimeIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

            var principle = new ClaimsPrincipal(identity);

            return SignIn(principle,
                new()
                {
                    RedirectUri = returnUrl,
                    IsPersistent = true
                },
                CookieAuthenticationDefaults.AuthenticationScheme);
        }

        [HttpGet("Register")]
        public IActionResult Register(string? returnUrl)
        {
            return View(new RegisterModel() { ReturnUrl = returnUrl });
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var identityuser = new IdentityUser() { Email = model.Email, UserName = model.Email };

            var result = await userManager.CreateAsync(identityuser, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            await userManager.AddToRoleAsync(identityuser, "PermanentUser");

            model.ReturnUrl = SetIfNullReturnUrl(model.ReturnUrl);

            return RedirectToAction("Login", new { returnUrl = model.ReturnUrl });
        }



        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {

                return View(model);
            }

            var user = await userManager.FindByEmailAsync(model.Email);

            if (user == null || !await userManager.CheckPasswordAsync(user, model.Password))
            {
                ModelState.AddModelError(string.Empty, INVALIDLOGGINGATTEMPTMESSAGE);

                return View(model);
            }

            await signInManager.SignInWithClaimsAsync(user, null,
                (await claimsFactory.GenerateRoleAdditionalClaims("PermanentUser")));

            model.ReturnUrl = SetIfNullReturnUrl(model.ReturnUrl);

            return LocalRedirect(model.ReturnUrl);
        }

        [HttpGet("Login")]
        public IActionResult Login(string? returnUrl)
        {
            return View(new LoginModel() { ReturnUrl = returnUrl });
        }

        [HttpGet("Leave")]
        public async Task<IActionResult> Leave()
        {
            await signInManager.SignOutAsync();

            return RedirectToAction("Login", new { returnUrl = "/" });
        }


        private string SetIfNullReturnUrl(string returnUrl)
        {
            return string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl;
        }

    }


}
