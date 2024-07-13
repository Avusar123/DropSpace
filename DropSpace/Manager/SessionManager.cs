using DropSpace.Models.Data;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace DropSpace.Manager
{
    public class SessionManager(ApplicationContext applicationContext, IConfiguration configuration, RoleManager<UserPlanRole> roleManager) : IManager<Session, Guid>
    {

        public async Task<Guid> CreateAsync(Session entity)
        {

            entity.Id = Guid.NewGuid();

            applicationContext.Add(entity);

            await applicationContext.SaveChangesAsync();

            return entity.Id;
        }

        public async Task<SessionMember> CreateDefaultNew(ClaimsPrincipal claimsPrincipal)
        {

            var userPlan = await roleManager.FindByNameAsync(claimsPrincipal.FindFirstValue(ClaimTypes.Role)!);

            var session = new Session
            {
                Created = DateTime.Now,
                Duration = TimeSpan.FromSeconds(userPlan!.SessionDuration),
                MaxSize = userPlan!.MaxSize,
                Name = configuration.GetValue<string>("SessionDefaultName")!
            };

            var id = await CreateAsync(session);

            var member = new SessionMember() 
            { 
                SessionId = id, 
                Status = MemberStatus.Owner, 
                UserId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)!.Value! 
            };

            session.Members.Add(member);

            await applicationContext.SaveChangesAsync();

            return member;
        }

        public async Task Delete(Guid key)
        {
            applicationContext.Remove(key);

            await applicationContext.SaveChangesAsync();
        }

        public async Task<Session> GetAsync(Guid key)
        {
            return (await applicationContext.FindAsync<Session>(key))!;
        }

        public async Task Update(Session entity)
        {
            applicationContext.Update(entity);

            await applicationContext.SaveChangesAsync();
        }
    }
}
