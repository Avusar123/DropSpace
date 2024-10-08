using DropSpace.Models.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace DropSpace
{
    public class ApplicationContext(DbContextOptions options) : IdentityDbContext<IdentityUser, UserPlanRole, string>(options)
    {
        public DbSet<Session> Sessions { get; set; }

        public DbSet<SessionMember> Members { get; set; }

        public DbSet<FileModel> Files { get; set; }

        public DbSet<PendingUploadModel> PendingUploads { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Session>()
                .HasMany<FileModel>(session => session.Files);

            builder.Entity<FileModel>()
                .HasOne<PendingUploadModel>(file => file.PendingUpload)
                .WithOne(upload => upload.File)
                .HasForeignKey<PendingUploadModel>(upload => upload.FileId);

            base.OnModelCreating(builder);
        }
    }
}
