using DropSpace.DataManagers;
using DropSpace.Manager;
using DropSpace.Models;
using DropSpace.Models.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics.Metrics;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace DropSpace.Controllers
{
    [Route("Auth")]
    public class AuthController(ClaimsFactory claimsFactory, 
        SignInManager<IdentityUser> signInManager, 
        UserManager<IdentityUser> userManager,
        SessionManager sessionManager,
        RoleManager<UserPlanRole> roleManager) : Controller
    {
        const string INVALID_LOGGING_ATTEMPT_MESSAGE = "Неудачная попытка входа";


        [HttpPost("OneTime")]
        public async Task<IActionResult> OneTimeRegister(string? returnUrl)
        {

            var identity = claimsFactory.CreateOneTimeIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

            var principle = new ClaimsPrincipal(identity);

            await sessionManager.CreateDefaultNew(principle);

            return SignIn(principle, 
                new() { 
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

            await signInManager.SignInAsync(identityuser, true);

            await sessionManager.CreateDefaultNew(HttpContext.User);

            model.ReturnUrl ??= "/";

            return LocalRedirect(model.ReturnUrl);
        }



        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {

                return View(model);
            }

            var user = await userManager.FindByEmailAsync(model.Email);

            if (user == null) 
            {
                ModelState.AddModelError(string.Empty, INVALID_LOGGING_ATTEMPT_MESSAGE);

                return View(model);
            }

            var result = await signInManager.PasswordSignInAsync(user!, model.Password, true, true);

            if (!result.Succeeded)
            {
                if (result.IsNotAllowed) 
                {
                    ModelState.AddModelError(string.Empty, "Вход не разрешен");
                }

                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Аккаут заблокирован! Попробуйте позже");

                    return View(model);
                    
                }

                ModelState.AddModelError(string.Empty, INVALID_LOGGING_ATTEMPT_MESSAGE);

                return View(model);
            }

            if (string.IsNullOrEmpty(model.ReturnUrl)) 
            {
                model.ReturnUrl = "/";
            }

            return LocalRedirect(model.ReturnUrl);
        }

        [HttpGet("Login")]
        public IActionResult Login(string? returnUrl)
        {
            return View(new LoginModel() { ReturnUrl = returnUrl});
        }

    }

    
}
