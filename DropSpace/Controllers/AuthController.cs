using DropSpace.DataManagers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace DropSpace.Controllers
{
    [Route("Auth")]
    public class AuthController(ClaimsFactory claimsFactory) : Controller
    {

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


        [HttpPost("LoginPersistant")]
        public IActionResult LoginPersistant(string? returnUrl) => Challenge(
            new AuthenticationProperties() { RedirectUri = returnUrl }, IdentityConstants.ApplicationScheme
        );

        [HttpGet("Choose")]
        public IActionResult Choose(string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
    }
}
