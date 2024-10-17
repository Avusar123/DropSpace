using DropSpace.WebApi.Controllers.Filters.MemberFilter.Providers;
using DropSpace.WebApi.Utils.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DropSpace.WebApi.Controllers.Filters.MemberFilter
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SessionMemberFilter(string paramName) : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                if (!context.ActionArguments.TryGetValue(paramName, out var model) || model == null)
                {
                    throw new NullReferenceException();
                }

                using var scope = context.HttpContext.RequestServices.CreateScope();

                Guid sessionId;

                if (model is Guid guid)
                {
                    sessionId = guid;
                }
                else
                {
                    var modelType = model.GetType();

                    var providerType = typeof(ISessionIdProvider<>).MakeGenericType(modelType);

                    var provider = scope.ServiceProvider
                            .GetRequiredService(providerType);

                    var method = providerType.GetMethod("GetFrom") 
                                ?? throw new ArgumentException("Не найдено реализации для заданного типа!");
                    
                    sessionId = (Guid)method.Invoke(provider, [model])!;
                }

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