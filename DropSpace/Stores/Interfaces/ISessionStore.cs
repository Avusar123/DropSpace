using DropSpace.Models.Data;

namespace DropSpace.Stores.Interfaces
{
    public interface ISessionStore
    {
        Task<Guid> CreateAsync(Session session);

        Task Delete(Guid id);

        Task<List<Session>> GetAll(string userId);

        Task<Session> GetAsync(Guid id, bool includeExpired = false);

        Task UpdateAsync(Session session);
    }
}
