using DropSpace.Models.Data;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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

            base.OnModelCreating(builder);
        }
    }
}
