using Microsoft.EntityFrameworkCore;
namespace ProjectManager.Models
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectUser> ProjectUsers { get; set; }
        public DbSet<User> Users { get;set; }  
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

    }
}
