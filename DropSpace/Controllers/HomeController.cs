using DropSpace.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.RateLimiting;

namespace DropSpace.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {

            return View();
        }
    }
}
