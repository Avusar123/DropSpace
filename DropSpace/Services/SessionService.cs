using DropSpace.Events.Events;
using DropSpace.Events.Interfaces;
using DropSpace.Models.Data;
using DropSpace.Models.DTOs;
using DropSpace.Providers;
using DropSpace.SignalRHubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
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
        RoleManager<UserPlanRole> roleManager,
        IEventTransmitter eventTransmitter) : ISessionService
    {

        public async Task<Guid> CreateAsync(Session entity)
        {

            entity.Id = Guid.NewGuid();

            applicationContext.Add(entity);

            await applicationContext.SaveChangesAsync();

            return entity.Id;
        }

        public async Task<SessionDto> CreateDefaultNew(ClaimsPrincipal claimsPrincipal, string sessionName)
        {
            var roleid = claimsPrincipal.FindFirstValue(ClaimTypes.Role)
                ?? throw new AuthenticationException("Id роли не найден!");

            var userPlan = await roleManager.FindByNameAsync(roleid)
                ?? throw new AuthenticationException("Роль не найдена!");

            var session = new Session()
            {
                Created = DateTime.Now,
                Duration = TimeSpan.FromSeconds(userPlan.SessionDuration),
                MaxSize = userPlan.MaxSize,
                Name = sessionName,
            };

            await CreateAsync(session);

            return new SessionDto(session.Id, session.Name, 0);
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

        public async Task<Session> GetAsync(Guid key, bool includeExpired = false)
        {
            return (await applicationContext
                .Sessions
                .Include(session => session.Files)
                .Include(session => session.Members)
                .Where(session => (session.Created + session.Duration > DateTime.Now) || includeExpired)
                .FirstOrDefaultAsync(s => s.Id == key)) ?? throw new NullReferenceException("Сессия не найдена!");
        }

        public async Task<SessionMember> JoinSession(ClaimsPrincipal claimsPrincipal, Guid key)
        {
            var session = await GetAsync(key);

            var roleid = claimsPrincipal.FindFirstValue(ClaimTypes.Role)
                ?? throw new AuthenticationException("Id роли не найден!");

            var userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new AuthenticationException("Id пользователя не найден!");

            var userPlan = await roleManager.FindByNameAsync(roleid)
                ?? throw new AuthenticationException("Роль не найдена!");

            if (!CanJoin(userId, userPlan))
            {
                throw new MaxSessionsLimitReached(userPlan.MaxSessions);
            }

            var member = new SessionMember()
            {
                UserId = userId
            };

            session.Members.Add(member);

            await eventTransmitter.FireEvent(new UserJoinedEvent() { Session = session, UserId = userId });

            applicationContext.SaveChanges();

            return member;
        }

        public async Task LeaveSession(ClaimsPrincipal claimsPrincipal, Guid key)
        {
            try
            {
                var session = await GetAsync(key);

                var userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new AuthenticationException("Id пользователя не найден!");

                var member = session.Members.FirstOrDefault(m => m.UserId == userId);

                if (member == null)
                {
                    return;
                }

                session.Members.Remove(member);

                if (session.Members.Count == 0)
                {
                    await Delete(key);
                }

                await eventTransmitter.FireEvent(new UserLeftEvent() { Session = session, UserId = userId });

                applicationContext.SaveChanges();

            }
            catch (NullReferenceException)
            {
                return;
            }
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
                    s => new SessionDto(s.Id, s.Name, s.Members.Count)
                )];
        }

        public async Task Update(Session entity)
        {
            applicationContext.Update(entity);

            await applicationContext.SaveChangesAsync();
        }

        private bool CanJoin(string userId, UserPlanRole userPlan)
        {
            return userPlan.MaxSessions > applicationContext
                .Sessions
                .Include(s => s.Members)
                .Where(session => session.Created + session.Duration > DateTime.Now
                    && session.Members.Any(m => m.UserId == userId))
                .Count();
        }
    }
}
