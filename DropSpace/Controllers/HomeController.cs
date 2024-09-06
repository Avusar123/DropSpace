using DropSpace.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DropSpace.Controllers
{
    [Authorize(Policy = "BaseAccess")]
    public class HomeController(SessionService sessionManager) : Controller
    {
        public IActionResult Index()
        {

            return View();
        }
    }
}
