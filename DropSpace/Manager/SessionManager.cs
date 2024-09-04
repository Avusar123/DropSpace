using DropSpace.Models.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace DropSpace.Manager
{
    public class SessionManager(ApplicationContext applicationContext, 
        IConfiguration configuration, 
        RoleManager<UserPlanRole> roleManager,
        UserManager<IdentityUser> userManager) : IManager<Session, Guid>
    {

        public async Task<Guid> CreateAsync(Session entity)
        {

            entity.Id = Guid.NewGuid();

            applicationContext.Add(entity);

            await applicationContext.SaveChangesAsync();

            return entity.Id;
        }

        public async Task<SessionMember> CreateDefaultNew(IdentityUser user)
        {
            var roles = await userManager.GetRolesAsync(user);

            var userPlan = await roleManager.FindByNameAsync(roles.First());

            return await AddSession(user.Id, userPlan);
        }


        public async Task<SessionMember> CreateDefaultNew(ClaimsPrincipal claimsPrincipal)
        {
            var userPlan = await roleManager.FindByNameAsync(claimsPrincipal.FindFirstValue(ClaimTypes.Role)!);

            return await AddSession(claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)!.Value, userPlan!);
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

        public List<Session> GetAllSessions(string userId)
        {
            var sessions = applicationContext.Sessions.Include(session => session.Members);

            return [..sessions.Where(session => session.Members.Any(member => member.UserId == userId) && session.Created + session.Duration > DateTime.Now)];
        }

        public async Task Update(Session entity)
        {
            applicationContext.Update(entity);

            await applicationContext.SaveChangesAsync();
        }

        private async Task<SessionMember> AddSession(string userId, UserPlanRole userPlan)
        {
            var session = new Session
            {
                Created = DateTime.Now,
                Duration = TimeSpan.FromSeconds(userPlan!.SessionDuration),
                MaxSize = userPlan!.MaxSize,
                Name = configuration.GetValue<string>("SessionDefaultName")!
            };

            var member = new SessionMember()
            {
                UserId = userId
            };

            session.Members.Add(member);

            await CreateAsync(session);

            return member;
        }
    }
}
