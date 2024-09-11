using DropSpace.Models.Data;
using DropSpace.Models.DTOs;
using System.Security.Claims;

namespace DropSpace.Services
{
    public interface ISessionService
    {
        Task<Guid> CreateAsync(Session entity);
        Task<SessionDto> CreateDefaultNew(ClaimsPrincipal claimsPrincipal, string sessionName);
        Task Delete(Guid key);
        List<SessionDto> GetAllSessions(string userId);
        Task<Session> GetAsync(Guid key, bool includeExpired = false);
        Task<SessionMember> JoinSession(ClaimsPrincipal claimsPrincipal, Guid key);
        Task LeaveSession(ClaimsPrincipal claimsPrincipal, Guid key);
        Task Update(Session entity);
    }
}