
using DropSpace.Manager;
using DropSpace.Models.Data;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace DropSpace
{
    public class DataSeed(IServiceProvider serviceProvider) : BackgroundService
    {

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = serviceProvider.CreateScope();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<UserPlanRole>>();

            var sessionManager = scope.ServiceProvider.GetRequiredService<SessionManager>();


            if (!roleManager.Roles.Any())
            {
                var permanentUserRole = new UserPlanRole()
                {
                    Name = "PermanentUser",
                    MaxSize = 3000,
                    MaxSessions = 3,
                    SessionDuration = (int)TimeSpan.FromMinutes(15).TotalSeconds
                };

                var oneTimeUserRole = new UserPlanRole()
                {
                    Name = "OneTimeUser",
                    MaxSessions = 1,
                    MaxSize = 1000,
                    SessionDuration = (int)TimeSpan.FromMinutes(5).TotalSeconds
                };

                await roleManager.CreateAsync(permanentUserRole);
                await roleManager.CreateAsync(oneTimeUserRole);

            }

            if (userManager.Users.Count() == 0)
            {

                var user = new IdentityUser() 
                { 
                    Email = "test@gmail.com", 
                    UserName = "test@gmail.com" 
                };


                var result = await userManager.CreateAsync(user, "Nitroxwar123!");

                if (!result.Succeeded)
                {
                    throw new Exception("Seed data error");
                }

                await userManager.AddToRoleAsync(user, "PermanentUser");


                var session = new Session()
                {
                    Id = Guid.NewGuid(),
                    Created = DateTime.Now,
                    Duration = TimeSpan.FromMinutes(99999), 
                    MaxSize = 1000,
                    Members = new List<SessionMember>()
                    {
                        new() { UserId = user.Id }
                    },
                    Name = "Test"
                };


                for (int i = 0; i < 20; i++)
                {
                    session.AttachFileToSession
                    (
                        new FileModel() { Id = Guid.NewGuid(), FileName = "Test.exe", FilePath = "~/Test", Size = 10 }
                    );
                }
                
                await sessionManager.CreateAsync(session);

            }
        }
    }
}
