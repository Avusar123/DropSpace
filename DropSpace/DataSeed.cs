
using Microsoft.AspNetCore.Identity;

namespace DropSpace
{
    public class DataSeed(IServiceProvider serviceProvider) : BackgroundService
    {

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = serviceProvider.CreateScope();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            if (roleManager.Roles.Count() == 0)
            {
                var permanentUserRole = new IdentityRole("PermanentUser");

                await roleManager.CreateAsync(permanentUserRole);
            }

            if (userManager.Users.Count() == 0)
            {

                var user = new IdentityUser() { 
                    Email = "test@gmail.com", 
                    UserName = "test@gmail.com" };


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
