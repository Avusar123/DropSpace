using DropSpace.Models.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace DropSpace
{
    public class ApplicationContext(DbContextOptions options) : IdentityDbContext<IdentityUser, UserPlanRole, string>(options)
    {
        public DbSet<Session> Sessions { get; set; }

        public DbSet<SessionMember> Members { get; set; }

        public DbSet<FileModel> Files { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Session>().HasMany<FileModel>(session => session.Files);

            builder.Entity<SessionMember>().HasOne<Session>().WithMany(session => session.Members).HasForeignKey(member => member.SessionId);

            base.OnModelCreating(builder);
        }
    }
}
