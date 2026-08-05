using EMR.Domain.Entities;
using EMR.Shared.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EMR.Infrastructure.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

            try
            {
                // Auto-migrate database (Creates tables if they don't exist in cloud DB)
                await context.Database.MigrateAsync();

                // Ensure Roles exist
                if (!await context.Roles.AnyAsync(r => r.RoleId == 3))
                {
                    context.Roles.Add(new Role { RoleId = 3, RoleName = "Receptionist" });
                    await context.SaveChangesAsync();
                }

                // Check for Receptionist user
                if (!await context.Users.AnyAsync(u => u.Email == "reception@gmail.com"))
                {
                    PasswordHasher.CreateHash("Reception@123", out var passwordHash, out var passwordSalt);

                    var receptionist = new User
                    {
                        FullName = "Front Desk Reception",
                        Email = "reception@gmail.com",
                        PasswordHash = passwordHash,
                        PasswordSalt = passwordSalt,
                        RoleId = 3, // Receptionist
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    context.Users.Add(receptionist);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Receptionist user seeded successfully.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }
    }
}
