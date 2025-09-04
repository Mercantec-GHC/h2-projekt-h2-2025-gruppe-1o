using API.Data;
using Bogus;
using DomainModels;
using Microsoft.EntityFrameworkCore;
using FullBCrypt = BCrypt.Net.BCrypt;

namespace API.Services
{
    public class DataSeederService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<DataSeederService> _logger;

        public DataSeederService(AppDBContext context, ILogger<DataSeederService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ClearDatabaseAsync()
        {
            // Først slettes bookinger for at undgå foreign key-konflikter
            var bookings = await _context.Bookings.ToListAsync();
            if (bookings.Any())
            {
                _context.Bookings.RemoveRange(bookings);
            }

            // Slet kun brugere med rollen "User" (gæster)
            var users = await _context.Users.Where(u => u.Role != null && u.Role.Name == "User").ToListAsync();
            if (users.Any())
            {
                _context.Users.RemoveRange(users);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Slettet genererede Bookings og Gæste-brugere fra databasen.");
        }

        public async Task SeedDataAsync(int userCount, int bookingCount)
        {
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (userRole == null)
            {
                throw new InvalidOperationException("Systemfejl: Standardrollen 'User' findes ikke.");
            }

            _logger.LogInformation("Starter seeding af data...");

            // Hent listen over alle e-mails, der allerede findes i databasen.
            var existingEmails = await _context.Users.Select(u => u.Email).ToHashSetAsync();

            var users = GenerateUniqueUsers(userCount, userRole.Id, existingEmails);
            if (users.Any())
            {
                await _context.Users.AddRangeAsync(users);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Genereret og gemt {Count} nye brugere.", users.Count);
            }
            else
            {
                _logger.LogInformation("Ingen nye brugere blev tilføjet.");
            }

            var roomTypes = await _context.RoomTypes.ToListAsync();
            var userIds = await _context.Users.Where(u => u.RoleId == userRole.Id).Select(u => u.Id).ToListAsync();

            if (userIds.Any())
            {
                var bookings = GenerateBookings(bookingCount, userIds, roomTypes);
                await _context.Bookings.AddRangeAsync(bookings);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Genereret og gemt {Count} bookinger.", bookings.Count);
            }
            else
            {
                _logger.LogInformation("Ingen gæster fundet, skipper booking-seeding.");
            }
        }

        private List<User> GenerateUniqueUsers(int count, string userRoleId, HashSet<string> existingEmails)
        {
            var newUsers = new List<User>();
            var faker = new Faker("da"); // Bruger dansk lokalisation for navne

            for (int i = 0; i < count; i++)
            {
                string email;
                string firstName = faker.Name.FirstName();
                string lastName = faker.Name.LastName();

                do
                {
                    email = faker.Internet.Email(firstName, lastName).ToLower();
                } while (existingEmails.Contains(email));

                existingEmails.Add(email);

                var newUser = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    // RETTELSE: Genererer et simpelt 8-cifret nummer, der altid passer.
                    PhoneNumber = faker.Phone.PhoneNumber("########"),
                    PasswordBackdoor = "Password123!",
                    HashedPassword = FullBCrypt.HashPassword("Password123!"),
                    RoleId = userRoleId,
                    LastLogin = faker.Date.Recent(60).ToUniversalTime(),
                    CreatedAt = faker.Date.Past(1).ToUniversalTime(),
                    UpdatedAt = DateTime.UtcNow
                };
                newUsers.Add(newUser);
            }
            return newUsers;
        }

        private List<Booking> GenerateBookings(int count, List<string> userIds, List<RoomType> roomTypes)
        {
            var bookings = new List<Booking>();
            var faker = new Faker();

            for (int i = 0; i < count; i++)
            {
                var roomType = faker.PickRandom(roomTypes);
                var nights = faker.Random.Int(1, 7);
                var checkInDate = faker.Date.Future(1).Date;
                var checkOutDate = checkInDate.AddDays(nights);

                var newBooking = new Booking
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = faker.PickRandom(userIds),
                    RoomTypeId = roomType.Id,
                    CheckInDate = checkInDate.ToUniversalTime(),
                    CheckOutDate = checkOutDate.ToUniversalTime(),
                    TotalPrice = roomType.BasePrice * nights,
                    Status = "Confirmed"
                };
                bookings.Add(newBooking);
            }
            return bookings;
        }
    }
}