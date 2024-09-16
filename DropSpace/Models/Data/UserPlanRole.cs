using Microsoft.AspNetCore.Identity;

namespace DropSpace.Models.Data
{
    public class UserPlanRole : IdentityRole
    {
        public long MaxSize { get; set; }

        public int MaxSessions { get; set; }

        public int SessionDuration { get; set; }
    }
}
