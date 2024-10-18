using DropSpace.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace DropSpace.Infrastructure
{
    public class ApplicationContext : IdentityDbContext<IdentityUser, UserPlanRole, string>
    {
        public DbSet<Session> Sessions { get; set; }

        public DbSet<SessionMember> Members { get; set; }

        public DbSet<FileModel> Files { get; set; }

        public ApplicationContext(DbContextOptions options) : base(options)
        {
            Database.Migrate();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Session>()
                .HasMany(session => session.Files);

            base.OnModelCreating(builder);
        }
    }
}
