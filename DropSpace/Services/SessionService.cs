using DropSpace.Contracts.Dtos;
using DropSpace.Events.Events;
using DropSpace.Events.Interfaces;
using DropSpace.Extensions;
using DropSpace.Files.Interfaces;
using DropSpace.Models.Data;
using DropSpace.Services.Interfaces;
using DropSpace.Stores.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Security.Authentication;
using System.Security.Claims;
namespace DropSpace.Services
{
    public class MaxSessionsLimitReached(int limit)
        : Exception(string.Format("Лимит в {0} сессий превышен!", limit))
    {
    }

    public class SessionService(
        ISessionStore sessionStore,
        IFileVault fileVault,
        RoleManager<UserPlanRole> roleManager,
        IEventTransmitter eventTransmitter) : ISessionService
    {
        public async Task<SessionDto> CreateFromPrincipalAsync(ClaimsPrincipal claimsPrincipal, string sessionName)
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

            await sessionStore.CreateAsync(session);

            return session.ToDto();
        }

        public async Task Delete(Guid key)
        {
            var session = await sessionStore.GetAsync(key, true);

            await sessionStore.Delete(key);

            foreach (var file in session.Files)
            {
                await fileVault.DeleteAsync(file.Id.ToString());
            }
        }

        public async Task<Session> GetAsync(Guid key, bool includeExpired = false)
        {
            return await sessionStore.GetAsync(key, includeExpired);
        }

        public async Task<SessionMember> JoinSession(ClaimsPrincipal claimsPrincipal, Guid key)
        {
            var session = await GetAsync(key);

            var userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new AuthenticationException("Id пользователя не найден!");

            await ThrowIfCannotJoin(claimsPrincipal);

            var member = new SessionMember()
            {
                UserId = userId
            };

            session.Members.Add(member);

            await eventTransmitter.FireEvent(new UserJoinedEvent() { Session = session, UserId = userId });

            await sessionStore.UpdateAsync(session);

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

                await eventTransmitter.FireEvent(new UserLeftEvent() { Session = session, UserId = userId });

                if (session.Members.Count == 0)
                {
                    await Delete(key);
                }
                else
                {
                    await Update(session);
                }

            }
            catch (NullReferenceException)
            {
                return;
            }
        }

        public async Task<List<SessionDto>> GetAllSessions(string userId)
        {

            return (await sessionStore
                .GetAll(userId))
                .Select(
                    s => s.ToDto()
                ).ToList();
        }

        public async Task Update(Session entity)
        {
            await sessionStore.UpdateAsync(entity);
        }
        public async Task<bool> CanSave(Guid sessionId, long size)
        {
            var session = await GetAsync(sessionId);

            var totalSize = session
                            .Files
                            .Select(file =>
                            {
                                if (file.PendingUpload != null && file.PendingUpload.IsCompleted)
                                {
                                    return file.ByteSize;
                                }

                                return file.PendingUpload?.SendedSize ?? 0;
                            })
                            .Sum();

            return session.MaxSize - totalSize >= size;
        }

        public async Task ThrowIfCannotJoin(ClaimsPrincipal claimsPrincipal)
        {
            var roleid = claimsPrincipal.FindFirstValue(ClaimTypes.Role)
                ?? throw new AuthenticationException("Id роли не найден!");

            var userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new AuthenticationException("Id пользователя не найден!");

            var userPlan = await roleManager.FindByNameAsync(roleid)
                ?? throw new AuthenticationException("Роль не найдена!");

            if (!(userPlan.MaxSessions > (await GetAllSessions(userId)).Count))
            {
                throw new MaxSessionsLimitReached(userPlan.MaxSessions);
            }
        }
    }
}
