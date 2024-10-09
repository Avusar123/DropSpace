using DropSpace.WebApi.Utils.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DropSpace.WebApi.Controllers.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SessionMemberFilter(string paramName, string? nestedName = null) : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                if (!context.ActionArguments.TryGetValue(paramName, out var model) || model == null)
                {
                    throw new NullReferenceException();
                }

                Guid sessionId;

                if (model is Guid guid)
                {
                    sessionId = guid;
                }
                else
                {
                    var prop = model.GetType().GetProperty(nestedName!);

                    sessionId = (Guid)prop!.GetValue(model)!;
                }

                using var scope = context.HttpContext.RequestServices.CreateScope();

                var authService = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();

                var result = await authService.AuthorizeAsync(context.HttpContext.User, sessionId, new MemberRequirement());

                if (!result.Succeeded)
                {
                    context.Result = new ForbidResult();

                    return;
                }

                await next();
            }
            catch (NullReferenceException)
            {
                context.Result = new BadRequestResult();

                return;
            }
        }
    }

}