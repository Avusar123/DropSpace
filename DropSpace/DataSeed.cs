
using DropSpace.Models.Data;
using Microsoft.AspNetCore.Identity;

namespace DropSpace
{
    public class DataSeed(IServiceProvider serviceProvider) : BackgroundService
    {

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = serviceProvider.CreateScope();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<UserPlanRole>>();


            if (!roleManager.Roles.Any())
            {
                var permanentUserRole = new UserPlanRole()
                {
                    Name = "PermanentUser",
                    MaxSize = 3000,
                    MaxSessions = 3,
                    SessionDuration = TimeSpan.FromMinutes(15).Seconds
                };

                var oneTimeUserRole = new UserPlanRole()
                {
                    Name = "OneTimeUser",
                    MaxSessions = 1,
                    MaxSize = 1000,
                    SessionDuration = TimeSpan.FromMinutes(5).Seconds
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
            }
        }
    }
}
