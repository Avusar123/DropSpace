using DropSpace.Contracts.Dtos;
using DropSpace.Models.Data;

namespace DropSpace.Extensions
{
    public static class SessionExtensions
    {
        public static List<string> GetMemberIds(this Session session)
        {
            return session.Members.Select(m => m.UserId).ToList();
        }

        public static SessionDto ToDto(this Session session)
        {
            return new SessionDto(session.Id, session.Name, session.Members.Count, (session.Created + session.Duration) - DateTime.Now);
        }
    }
}
