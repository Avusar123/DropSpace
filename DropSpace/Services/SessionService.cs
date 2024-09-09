using DropSpace.Models.Data;
using DropSpace.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Authentication;
using System.Security.Claims;
namespace DropSpace.Services
{
    public class MaxSessionsLimitReached(int limit) 
        : Exception(string.Format("Лимит в {0} сессий превышен!", limit))
    {
    }

    public class SessionService(ApplicationContext applicationContext,
        RoleManager<UserPlanRole> roleManager) : IService<Session, Guid>
    {

        public async Task<Guid> CreateAsync(Session entity)
        {

            entity.Id = Guid.NewGuid();

            applicationContext.Add(entity);

            await applicationContext.SaveChangesAsync();

            return entity.Id;
        }

        public async Task<SessionMember> CreateDefaultNew(ClaimsPrincipal claimsPrincipal, string sessionName)
        {
            var roleid = claimsPrincipal.FindFirstValue(ClaimTypes.Role)
                ?? throw new AuthenticationException("Id роли не найден!");

            var userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new AuthenticationException("Id пользователя не найден!");

            var userPlan = await roleManager.FindByNameAsync(roleid)
                ?? throw new AuthenticationException("Роль не найдена!");

            if (userPlan.MaxSessions <= applicationContext
                .Sessions
                .Include(s => s.Members)
                .Where(session => session.Created + session.Duration > DateTime.Now
                    && session.Members.Any(m => m.UserId == userId))
                .Count())
            {
                throw new MaxSessionsLimitReached(userPlan.MaxSessions); 
            }

            return await AddSession(claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)!.Value, userPlan!, sessionName);
        }

        public async Task Delete(Guid key)
        {
            var session = applicationContext.Sessions.FirstOrDefault(s => s.Id == key);

            if (session == null)
            {
                return;
            }

            applicationContext.Sessions.Remove(session);

            await applicationContext.SaveChangesAsync();
        }

        public async Task<Session> GetAsync(Guid key)
        {
            return (await applicationContext
                .Sessions
                .Include(session => session.Files)
                .Include(session => session.Members)
                .Where(session => session.Created + session.Duration > DateTime.Now)
                .FirstOrDefaultAsync(s => s.Id == key)) ?? throw new NullReferenceException("Сессия не найдена!");
        }

        public List<SessionDto> GetAllSessions(string userId)
        {
            var sessions = applicationContext.Sessions.Include(session => session.Members);

            return [.. sessions
                .Where(session => 
                    session
                        .Members
                        .Any(member => member.UserId == userId) 
                            && session.Created + session.Duration > DateTime.Now)
                .Select(
                    s => new SessionDto(s.Id, s.Name)
                )];
        }

        public async Task Update(Session entity)
        {
            applicationContext.Update(entity);

            await applicationContext.SaveChangesAsync();
        }

        private async Task<SessionMember> AddSession(string userId, UserPlanRole userPlan, string sessionName)
        {
            var session = new Session
            {
                Created = DateTime.Now,
                Duration = TimeSpan.FromSeconds(userPlan!.SessionDuration),
                MaxSize = userPlan!.MaxSize,
                Name = sessionName
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
