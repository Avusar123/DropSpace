using DropSpace.Models;
using DropSpace.Models.Data;
using DropSpace.Models.DTOs;
using DropSpace.Requirements;
using DropSpace.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DropSpace.Controllers
{
    [Route("Session")]
    [Authorize]
    public class SessionController(SessionService sessionService, IAuthorizationService authorizationService) : Controller
    {

        [HttpGet("{id}")]
        public async Task<ActionResult> Details(Guid id)
        {
            try
            {
                var session = await sessionService.GetAsync(id);

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    return Forbid();
                }

                if (session.Created + session.Duration < DateTime.Now)
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

                    await sessionService.Update(session);
                }

                return View(session);

            } catch (NullReferenceException)
            {
                return NotFound();
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
                var member = await sessionService.CreateDefaultNew(User, createSessionModel.Name);

                return Json(new SessionDto(member.SessionId, createSessionModel.Name));
            } catch (Exception er)
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
                var session = await sessionService.GetAsync(id);

                var member = session.Members
                    .FirstOrDefault(member => member.UserId == User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                if (member != null)
                    session.Members.Remove(member);

                if (session.Members.Count == 0)
                {
                    await sessionService.Delete(id);
                } else
                {
                    await sessionService.Update(session);
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