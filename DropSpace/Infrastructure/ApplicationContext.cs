using DropSpace.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace DropSpace.Infrastructure
{
    public class ApplicationContext(DbContextOptions options) : IdentityDbContext<IdentityUser, UserPlanRole, string>(options)
    {
        public DbSet<Session> Sessions { get; set; }

        public DbSet<SessionMember> Members { get; set; }

        public DbSet<FileModel> Files { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Session>()
                .HasMany(session => session.Files);

            base.OnModelCreating(builder);
        }
    }
}
