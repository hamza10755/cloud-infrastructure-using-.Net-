using cloud_infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace cloud_infrastructure.Data
{
    public static class IdentitySeeder
    {
        private static readonly string[] RequiredRoles = ["Admin", "Developer"];

        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Developer>>();

            await EnsureRolesAsync(roleManager);
            await EnsureAdminExistsAsync(dbContext, userManager);
            await EnsureSoftwarePackagesAsync(dbContext);
        }

        private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var role in RequiredRoles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task EnsureAdminExistsAsync(ApplicationDbContext dbContext, UserManager<Developer> userManager)
        {
            var admins = await userManager.GetUsersInRoleAsync("Admin");
            if (admins.Count > 0)
            {
                return;
            }

            var firstUser = await dbContext.Users
                .OrderBy(user => user.Id)
                .FirstOrDefaultAsync();

            if (firstUser != null)
            {
                await userManager.AddToRoleAsync(firstUser, "Admin");
            }
        }

        private static async Task EnsureSoftwarePackagesAsync(ApplicationDbContext dbContext)
        {
            if (!await dbContext.SoftwarePackages.AnyAsync())
            {
                dbContext.SoftwarePackages.AddRange(
                    new SoftwarePackage { PackageName = "Docker", Version = "24.0.7" },
                    new SoftwarePackage { PackageName = "Node.js", Version = "20.11.0" },
                    new SoftwarePackage { PackageName = "Python", Version = "3.12.1" },
                    new SoftwarePackage { PackageName = "PostgreSQL", Version = "16.1" },
                    new SoftwarePackage { PackageName = "Nginx", Version = "1.25.3" }
                );
                await dbContext.SaveChangesAsync();
            }
        }
    }
}