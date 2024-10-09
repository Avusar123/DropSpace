using Microsoft.AspNetCore.Identity;

namespace DropSpace.Domain
{
    public class UserPlanRole : IdentityRole
    {
        public long MaxSize { get; set; }

        public int MaxSessions { get; set; }

        public int SessionDuration { get; set; }
    }
}
