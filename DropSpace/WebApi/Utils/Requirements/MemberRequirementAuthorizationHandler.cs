using DropSpace.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DropSpace.WebApi.Utils.Requirements
{
    public class MemberRequirementAuthorizationHandler(ApplicationContext applicationContext) : AuthorizationHandler<MemberRequirement, Guid>
    {

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, MemberRequirement requirement, Guid resource)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                context.Fail();

                return;
            }

            var member = await applicationContext.Members.FirstOrDefaultAsync(member =>
            member.UserId == userIdClaim.Value
            && member.SessionId == resource);

            if (member == null)
            {
                context.Fail();
                return;
            }

            context.Succeed(requirement);
        }
    }
}
