using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TheOffice.Models;

namespace TheOffice.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers;
        public DbSet<Team> Teams;
        public DbSet<UserTeam> UserTeams;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserTeam>()
                .HasKey(x => new { x.Id, x.UserId, x.TeamId });

            builder.Entity<UserTeam>()
                .HasOne(x => x.User)
                .WithMany(x => x.UserTeams)
                .HasForeignKey(x => x.UserId);

            builder.Entity<UserTeam>()
                .HasOne(x => x.Team)
                .WithMany(x => x.UserTeams)
                .HasForeignKey(x => x.TeamId);
        }
    }
}