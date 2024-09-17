using DropSpace.Models.Data;

namespace DropSpace.Extensions
{
    public static class SessionExtensions
    {
        public static List<string> GetMemberIds(this Session session)
        {
            return session.Members.Select(m => m.UserId).ToList();
        }
    }
}
