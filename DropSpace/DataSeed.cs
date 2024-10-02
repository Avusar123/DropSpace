using DropSpace.Models.Data;
using DropSpace.Stores.Interfaces;
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

            var sessionStore = scope.ServiceProvider.GetRequiredService<ISessionStore>();


            if (!roleManager.Roles.Any())
            {
                var permanentUserRole = new UserPlanRole()
                {
                    Name = "PermanentUser",
                    MaxSize = (long)3000 * 1024 * 1024,
                    MaxSessions = 3,
                    SessionDuration = (int)TimeSpan.FromMinutes(15).TotalSeconds
                };

                var oneTimeUserRole = new UserPlanRole()
                {
                    Name = "OneTimeUser",
                    MaxSessions = 1,
                    MaxSize = (long)1000 * 1024 * 1024,
                    SessionDuration = (int)TimeSpan.FromMinutes(1).TotalSeconds
                };

                await roleManager.CreateAsync(permanentUserRole);
                await roleManager.CreateAsync(oneTimeUserRole);

            }
        }
    }
}
