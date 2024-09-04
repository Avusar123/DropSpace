using DropSpace.Manager;
using DropSpace.Models.Data;
using DropSpace.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DropSpace.Controllers
{
    [Route("Session")]
    [Authorize]
    public class SessionController(SessionManager sessionManager, IAuthorizationService authorizationService) : Controller
    {

        [HttpGet("{id}")]
        public async Task<ActionResult> Details(Guid id)
        {
            try
            {
                var session = await sessionManager.GetAsync(id);

                if (session.GetExpiresAt() < DateTime.Now)
                {
                    return NotFound();
                }

                var result = await authorizationService.AuthorizeAsync(User, session, new MemberRequirement());

                if (!result.Succeeded)
                {
                    return Forbid();
                }

                return View(session);

            } catch (NullReferenceException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create()
        {
            var member = await sessionManager.CreateDefaultNew(User);

            return RedirectToAction("Details", new { id = member.SessionId });
        }

        // POST: SessionController/Edit/5
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Guid id)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [HttpGet("Join/{id}")]
        public ActionResult Join(Guid id)
        {
            throw new NotSupportedException();
        }

        [HttpPost("Join/{id}")]
        [ValidateAntiForgeryToken]
        public ActionResult Join(Guid id, [FromForm]  bool agree)
        {
            throw new NotSupportedException();
        }

        [HttpDelete("{id}")]
        public ActionResult Leave(Guid id)
        {
            throw new NotSupportedException();
        }
    }
}
