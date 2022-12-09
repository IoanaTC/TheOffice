using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TheOffice.Models;
using Task = TheOffice.Models.Task;

namespace TheOffice.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Project> Projects;
        public DbSet<Comment> Comments;
        public DbSet<Task> Tasks;

    }
}