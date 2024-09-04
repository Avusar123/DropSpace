using DropSpace.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DropSpace.Requirements
{
    public class MemberRequirementAuthorizationHandler(ApplicationContext applicationContext) : AuthorizationHandler<MemberRequirement, Session>
    {

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, MemberRequirement requirement, Session resource)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                context.Fail();

                return;
            }

            var member = await applicationContext.Members.FirstOrDefaultAsync(member => 
            member.UserId == userIdClaim.Value 
            && member.SessionId == resource.Id);

            if (member == null)
            {
                context.Fail();
                return;
            }

            context.Succeed(requirement);
        }
    }
}
