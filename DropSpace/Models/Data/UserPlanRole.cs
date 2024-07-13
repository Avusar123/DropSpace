﻿using Microsoft.AspNetCore.Identity;

namespace DropSpace.Models.Data
{
    public class UserPlanRole : IdentityRole
    {
        public int MaxSize { get; set; }

        public int MaxSessions { get; set; }

        public int SessionDuration { get; set; }
    }
}
