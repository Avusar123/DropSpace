using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DropSpace
{
    public class ApplicationContext(DbContextOptions options) : IdentityDbContext<IdentityUser>(options)
    {

    }
}
