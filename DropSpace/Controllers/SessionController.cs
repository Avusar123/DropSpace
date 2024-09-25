using DropSpace.Extensions;
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
        IAuthorizationService authorizationService) : ControllerBase
    {

        [HttpGet("{id}")]
        public async Task<ActionResult> Details(Guid id)
        {
            try
            {
                var session = await sessionService.GetAsync(id);

                if (session.Created + session.Duration < DateTime.Now)
                {
                    return NotFound();
                }

                var result = await authorizationService.AuthorizeAsync(User, session.Id, new MemberRequirement());

                if (!result.Succeeded)
                {
                    await sessionService.JoinSession(User, id);
                }

                return Ok(session.ToDto());

            }
            catch (AuthenticationException)
            {
                return Challenge();
            }
        }

        [HttpPost]
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

                return Ok(sessionDto);

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

                return Ok();
            }
            catch (AuthenticationException)
            {
                return Challenge();
            }
        }
    }
}