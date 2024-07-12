using DropSpace.DataManagers;
using DropSpace.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace DropSpace.Controllers
{
    [Route("Auth")]
    public class AuthController(ClaimsFactory claimsFactory, 
        SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager) : Controller
    {
        const string INVALID_LOGGING_ATTEMPT_MESSAGE = "Неудачная попытка входа";


        [HttpPost("OneTime")]
        public IActionResult OneTimeRegister(string? returnUrl)
        {

            var identity = claimsFactory.CreateOneTimeIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

            return SignIn(new ClaimsPrincipal(identity), 
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

            await signInManager.SignInAsync(identityuser, true);

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
