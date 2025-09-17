using DomainModels;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public static class DataSeeder
    {
        public static async Task InitializeDatabaseAsync(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDBContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDBContext>>();

            try
            {
                // Kør eventuelle ventende migrationer
                await context.Database.MigrateAsync();

                // Seed de nødvendige medarbejderbrugere
                await SeedStaffUsersAsync(context, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        private static async Task SeedStaffUsersAsync(AppDBContext context, ILogger logger)
        {
            // Tjek om der allerede findes brugere for at undgå at køre dette hver gang
            if (await context.Users.AnyAsync(u => u.Email.EndsWith("@hotel.dk")))
            {
                logger.LogInformation("Staff users already exist. Skipping seed.");
                return;
            }

            logger.LogInformation("Seeding initial staff users...");

            // Find rollerne
            var managerRole = await context.Roles.SingleAsync(r => r.Name == "Manager");
            var receptionistRole = await context.Roles.SingleAsync(r => r.Name == "Receptionist");
            var housekeepingRole = await context.Roles.SingleAsync(r => r.Name == "Housekeeping");

            // Generer hash dynamisk!
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");

            var usersToSeed = new List<User>
            {
                new User
                {
                    Id = Guid.NewGuid().ToString(),
                    FirstName = "Manager",
                    LastName = "Admin",
                    Email = "manager@hotel.dk",
                    RoleId = managerRole.Id,
                    HashedPassword = passwordHash
                },
                new User
                {
                    Id = Guid.NewGuid().ToString(),
                    FirstName = "Receptionist",
                    LastName = "Test",
                    Email = "receptionist@hotel.dk",
                    RoleId = receptionistRole.Id,
                    HashedPassword = passwordHash
                },
                new User
                {
                    Id = Guid.NewGuid().ToString(),
                    FirstName = "Housekeeping",
                    LastName = "Test",
                    Email = "housekeeping@hotel.dk",
                    RoleId = housekeepingRole.Id,
                    HashedPassword = passwordHash
                }
            };

            await context.Users.AddRangeAsync(usersToSeed);
            await context.SaveChangesAsync();

            logger.LogInformation("Successfully seeded {Count} staff users.", usersToSeed.Count);
        }
    }
}