//namespace SessionManagementDemo.Models
//{
//    public class ApplicationDbContext
//    {
//    }
//}

using Microsoft.EntityFrameworkCore;
using SessionManagementDemo.Models;

namespace SessionManagementDemo.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}
