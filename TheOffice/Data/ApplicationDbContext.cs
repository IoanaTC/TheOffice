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

        public DbSet<Project> Projects { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<UserProject> UserProjects { get; set; }
        public DbSet<Status> Statuses { get; set; } 

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserProject>()
                .HasKey(x => new { x.Id, x.UserId, x.ProjectId });

            builder.Entity<UserProject>()
                .HasOne(x => x.User)
                .WithMany(x => x.UserProjects)
                .HasForeignKey(x => x.UserId);

            builder.Entity<UserProject>()
                .HasOne(x => x.Project)
                .WithMany(x => x.UserProjects)
                .HasForeignKey(x => x.ProjectId);
        }
    }
}