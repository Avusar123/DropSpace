using DropSpace.Contracts.Dtos;
using DropSpace.Contracts.Models;
using DropSpace.Logic.Extensions;
using DropSpace.Logic.Services.Interfaces;
using DropSpace.WebApi.Utils.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Authentication;
using System.Security.Claims;

namespace DropSpace.WebApi.Controllers
{
    [Route("Session")]
    [EnableRateLimiting("fixed")]
    [Authorize]
    [ApiController]
    public class SessionController(ISessionService sessionService,
            IAuthorizationService authorizationService) : ControllerBase
    {

        [HttpGet("{id}")]
        public async Task<ActionResult<SessionDto>> Details(Guid id)
        {
            try
            {
                var session = await sessionService.GetAsync(id);

                if (session.Created + session.Duration < DateTime.UtcNow)
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
            catch (NullReferenceException)
            {
                return NotFound(id);
            }
            catch (AuthenticationException)
            {
                return Challenge();
            }
            catch (Exception er)
            {
                ModelState.AddModelError(string.Empty, er.Message);

                return BadRequest(ModelState);
            }
        }

        [HttpGet("All")]
        public async Task<ActionResult<List<SessionDto>>> GetAll()
        {
            try
            {
                return await sessionService.GetAllSessions(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            }
            catch (Exception er)
            {
                ModelState.AddModelError(string.Empty, er.Message);

                return BadRequest(ModelState);
            }
        }

        [HttpPost]
        public async Task<ActionResult<SessionDto>> Create(CreateSessionModel createSessionModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await sessionService.ThrowIfCannotJoin(User);

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
            catch (Exception er)
            {
                ModelState.AddModelError(string.Empty, er.Message);

                return BadRequest(ModelState);
            }
        }
    }
}