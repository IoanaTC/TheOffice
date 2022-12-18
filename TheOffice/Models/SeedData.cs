using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheOffice.Data;

namespace TheOffice.Models
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService
                <DbContextOptions<ApplicationDbContext>>()))
            {
                // Verificam daca in baza de date exista cel putin un rol sau status
                if (context.Roles.Any() && context.Statuses.Any())
                {
                    return;
                }

                if (context.Roles.Any() == false)
                {
                    // Daca baza de date nu contine roluri, atunci se creeaza 
                    context.Roles.AddRange(
                        new IdentityRole { Id = "d9f258b2-3309-4c15-b73f-78652eaf5eb0", Name = "Admin", NormalizedName = "Admin".ToUpper() },
                        new IdentityRole { Id = "d9f258b2-3309-4c15-b73f-78652eaf5eb1", Name = "Organizator", NormalizedName = "Organizator".ToUpper() },
                        new IdentityRole { Id = "d9f258b2-3309-4c15-b73f-78652eaf5eb2", Name = "User", NormalizedName = "User".ToUpper() }
                    );


                    var hasher = new PasswordHasher<ApplicationUser>();


                    // Se creeaza cate un user pentru fiecare rol
                    context.Users.AddRange(
                        new ApplicationUser
                        {
                            Id = "2da6d48e-6a87-4157-a5e4-91ed719689e0",
                            UserName = "admin@test.com",
                            //FirstName = "Michael",
                            //LastName = "Admin",
                            //Birthday = DateTime.Now,
                            EmailConfirmed = true,
                            NormalizedEmail = "ADMIN@TEST.COM",
                            Email = "admin@test.com",
                            NormalizedUserName = "ADMIN@TEST.COM",
                            PasswordHash = hasher.HashPassword(null, "Admin1!")
                        },
                        new ApplicationUser
                        {
                            Id = "2da6d48e-6a87-4157-a5e4-91ed719689e1", // primary key
                            UserName = "organizator@test.com",
                            //FirstName = "Pam",
                            //LastName = "Organizator", 
                            //Birthday = DateTime.Now,
                            EmailConfirmed = true,
                            NormalizedEmail = "ORGANIZATOR@TEST.COM",
                            Email = "organizator@test.com",
                            NormalizedUserName = "ORGANIZATOR@TEST.COM",
                            PasswordHash = hasher.HashPassword(null, "Organizator1!")
                        },
                        new ApplicationUser
                        {
                            Id = "2da6d48e-6a87-4157-a5e4-91ed719689e2", // primary key
                            UserName = "user@test.com",
                            //FirstName = "User",
                            //LastName = "Kevin",
                            //Birthday = DateTime.Now,
                            EmailConfirmed = true,
                            NormalizedEmail = "USER@TEST.COM",
                            Email = "user@test.com",
                            NormalizedUserName = "USER@TEST.COM",
                            PasswordHash = hasher.HashPassword(null, "User1!")
                        }
                    );

                    // asocierea user cu rolul sau
                    context.UserRoles.AddRange(
                        new IdentityUserRole<string>
                        {
                            RoleId = "d9f258b2-3309-4c15-b73f-78652eaf5eb0",
                            UserId = "2da6d48e-6a87-4157-a5e4-91ed719689e0"
                        },
                        new IdentityUserRole<string>
                        {
                            RoleId = "d9f258b2-3309-4c15-b73f-78652eaf5eb1",
                            UserId = "2da6d48e-6a87-4157-a5e4-91ed719689e1"
                        },
                        new IdentityUserRole<string>
                        {
                            RoleId = "d9f258b2-3309-4c15-b73f-78652eaf5eb2",
                            UserId = "2da6d48e-6a87-4157-a5e4-91ed719689e2"
                        }
                    );

                }

                if (context.Statuses.Any() == false)
                {
                    context.Statuses.AddRange(

                        new Status
                        {
                            Status_Value = "Not Started"


                        },

                        new Status
                        {
                            Status_Value = "In Progress"


                        },

                        new Status
                        {
                            Status_Value = "Completed"


                        }

                    );
                }
                context.SaveChanges();
            }
        }

    }
}
