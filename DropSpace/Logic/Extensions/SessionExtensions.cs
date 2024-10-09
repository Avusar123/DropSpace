using DropSpace.Contracts.Dtos;
using DropSpace.Domain;

namespace DropSpace.Logic.Extensions
{
    public static class SessionExtensions
    {
        public static List<string> GetMemberIds(this Session session)
        {
            return session.Members.Select(m => m.UserId).ToList();
        }

        public static SessionDto ToDto(this Session session)
        {
            return new SessionDto(
                session.Id,
                session.Name,
                session.Members.Count,
                session.MaxSize,
                session.MaxSize.ToMBytes(),
                session.Created + session.Duration);
        }
    }
}
