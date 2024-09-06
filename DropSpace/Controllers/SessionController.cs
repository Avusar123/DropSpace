using DropSpace.Manager;
using DropSpace.Models;
using DropSpace.Models.Data;
using DropSpace.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    return Forbid();
                }

                if (session.GetExpiresAt() < DateTime.Now)
                {
                    return Redirect("/");
                }

                var result = await authorizationService.AuthorizeAsync(User, session, new MemberRequirement());

                if (!result.Succeeded)
                {
                    session.Members.Add(new SessionMember()
                    {
                        SessionId = session.Id,
                        UserId = userId
                    });

                    await sessionManager.Update(session);
                }

                return View(session);

            } catch (NullReferenceException)
            {
                return NotFound();
            }
        }

        [HttpGet("GetAll")]
        public ActionResult GetAllSessions()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Forbid();

            return Json(sessionManager.GetAllSessions(userId)
                .Select(session => new { session.Id, session.Name }));
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateSessionViewModel createSessionModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var member = await sessionManager.CreateDefaultNew(User, createSessionModel.Name);

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

        [HttpDelete("{id}")]
        public async Task<ActionResult> Leave(Guid id)
        {
            try
            {
                var session = await sessionManager.GetAsync(id);

                var member = session.Members
                    .FirstOrDefault(member => member.UserId == User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                if (member != null)
                    session.Members.Remove(member);

                if (session.Members.Count == 0)
                {
                    await sessionManager.Delete(id);
                } else
                {
                    await sessionManager.Update(session);
                }

                return Redirect("/");

            }
            catch (NullReferenceException)
            {
                return NotFound();
            }
        }
    }
}
