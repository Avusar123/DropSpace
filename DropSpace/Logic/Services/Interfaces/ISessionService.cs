using DropSpace.Contracts.Dtos;
using DropSpace.Domain;
using System.Security.Claims;

namespace DropSpace.Logic.Services.Interfaces
{
    public interface ISessionService
    {
        Task<SessionDto> CreateFromPrincipalAsync(ClaimsPrincipal claimsPrincipal, string sessionName);
        Task Delete(Guid key);
        Task<List<SessionDto>> GetAllSessions(string userId);
        Task<Session> GetAsync(Guid key, bool includeExpired = false);
        Task<SessionMember> JoinSession(ClaimsPrincipal claimsPrincipal, Guid key);
        Task LeaveSession(ClaimsPrincipal claimsPrincipal, Guid key);
        Task Update(Session entity);
        Task<bool> CanSave(Guid sessionId, long size, Guid? fileId = null);
        Task ThrowIfCannotJoin(ClaimsPrincipal claimsPrincipal);
    }
}