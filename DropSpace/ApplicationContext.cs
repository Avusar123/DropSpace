using Microsoft.EntityFrameworkCore;

namespace DropSpace
{
    public class ApplicationContext(DbContextOptions<ApplicationContext> options) : DbContext(options)
    {
    }
}
