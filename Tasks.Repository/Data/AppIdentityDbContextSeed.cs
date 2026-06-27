using Microsoft.AspNetCore.Identity;
using Tasks.Domain.Authorization;
using Tasks.Domain.Enums;
using Tasks.Domain.Models.Identity;

namespace Tasks.Repository.Data
{
    public static class AppIdentityDbContextSeed
    {
        public static async Task SeedUserAsync(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Seed roles first
            foreach (var role in new[] { Roles.Admin, Roles.Employee })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            if (!userManager.Users.Any())
            {
                var admin = new AppUser
                {
                    UserName       = "admin",
                    Email          = "admin@pixelsoft.com",
                    FirstName      = "System",
                    LastName       = "Admin",
                    Gender         = Gender.Male,
                    IsActive       = true,
                    PhoneNumber    = "01000000001",
                    EmailConfirmed = true,
                };
                await userManager.CreateAsync(admin, "Admin@123456");
                await userManager.AddToRoleAsync(admin, Roles.Admin);

                var abdelrahman = new AppUser
                {
                    UserName       = "abdelrahman",
                    Email          = "abdelrahmanmohamed2293@gmail.com",
                    FirstName      = "Abdelrahman",
                    LastName       = "Mohamed",
                    Gender         = Gender.Male,
                    IsActive       = true,
                    PhoneNumber    = "01015496488",
                    EmailConfirmed = true,
                };
                await userManager.CreateAsync(abdelrahman, "Ar115599@");
                await userManager.AddToRoleAsync(abdelrahman, Roles.Admin);

                var khaled = new AppUser
                {
                    UserName       = "khaled",
                    Email          = "khaledmramadan136@gmail.com",
                    FirstName      = "Khaled",
                    LastName       = "Ramadan",
                    Gender         = Gender.Male,
                    IsActive       = true,
                    PhoneNumber    = "01015496488",
                    EmailConfirmed = true,
                };
                await userManager.CreateAsync(khaled, "Ar115599@");
                await userManager.AddToRoleAsync(khaled, Roles.Employee);

                var omar = new AppUser
                {
                    UserName       = "omar",
                    Email          = "omar.ramadan2845@gmail.com",
                    FirstName      = "Omar",
                    LastName       = "Ramadan",
                    Gender         = Gender.Male,
                    IsActive       = true,
                    PhoneNumber    = "01015496488",
                    EmailConfirmed = true,
                };
                await userManager.CreateAsync(omar, "Ar115599@");
                await userManager.AddToRoleAsync(omar, Roles.Employee);
            }
        }
    }
}
