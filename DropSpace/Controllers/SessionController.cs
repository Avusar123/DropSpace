using DropSpace.Models;
using DropSpace.Requirements;
using DropSpace.Services;
using DropSpace.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Authentication;

namespace DropSpace.Controllers
{
    [Route("Session")]
    [EnableRateLimiting("fixed")]
    [Authorize]
    public class SessionController(ISessionService sessionService,
        IAuthorizationService authorizationService) : Controller
    {

        [HttpGet("{id}")]
        public async Task<ActionResult> Details(Guid id)
        {
            try
            {
                var session = await sessionService.GetAsync(id);

                if (session.Created + session.Duration < DateTime.Now)
                {
                    return Redirect("/");
                }

                var result = await authorizationService.AuthorizeAsync(User, session.Id, new MemberRequirement());

                if (!result.Succeeded)
                {
                    await sessionService.JoinSession(User, id);
                }

                return View(session);

            }
            catch (NullReferenceException)
            {
                return Redirect("/");

            }
            catch (AuthenticationException)
            {
                return Challenge();

            }
            catch (MaxSessionsLimitReached)
            {
                return View("LimitError");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateSessionViewModel createSessionModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var sessionDto = await sessionService.CreateFromPrincipalAsync(User, createSessionModel.Name);

                var member = await sessionService.JoinSession(User, sessionDto.Id);

                return Json(sessionDto);

            }
            catch (Exception er)
            {
                ModelState.AddModelError(string.Empty, er.Message);

                return BadRequest(ModelState);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Leave(Guid id)
        {
            try
            {
                await sessionService.LeaveSession(User, id);
            }
            catch (AuthenticationException)
            {
                return Challenge();
            }


            return Redirect("/");
        }

        [HttpGet("Invite")]
        public async Task<ActionResult> Invite(string code)
        {
            if (code == null)
                return Redirect("/");

            return View();
        }
    }
}