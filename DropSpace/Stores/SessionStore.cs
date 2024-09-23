using DropSpace.Models.Data;
using DropSpace.Stores.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DropSpace.Stores
{
    public class SessionStore(
        ApplicationContext applicationContext) : ISessionStore
    {
        public async Task<Guid> CreateAsync(Session session)
        {
            session.Id = Guid.NewGuid();

            applicationContext.Add(session);

            await applicationContext.SaveChangesAsync();

            return session.Id;
        }

        public async Task Delete(Guid id)
        {
            var session = await applicationContext.Sessions.FirstOrDefaultAsync(s => s.Id == id);

            if (session != null)
            {
                applicationContext.Remove(session);
            }

            await applicationContext.SaveChangesAsync();
        }

        public async Task<List<Session>> GetAll(string userId)
        {
            return await applicationContext
                .Sessions
                .Include(session => session.Members)
                .Where(session =>
                    session
                        .Members
                        .Any(member => member.UserId == userId)
                            && session.Created + session.Duration > DateTime.Now)
                .ToListAsync();
        }

        public async Task<Session> GetAsync(Guid id, bool includeExpired = false)
        {
            return (await applicationContext
                .Sessions
                .Include(session => session.Files)
                .Include(session => session.Members)
                .Where(session => (session.Created + session.Duration > DateTime.Now) || includeExpired)
                .FirstOrDefaultAsync(s => s.Id == id)) ?? throw new NullReferenceException("Сессия не найдена!");
        }

        public async Task UpdateAsync(Session session)
        {
            applicationContext.Update(session);

            await applicationContext.SaveChangesAsync();
        }
    }
}
