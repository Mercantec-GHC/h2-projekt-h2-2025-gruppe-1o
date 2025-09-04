using API.Data;
using Bogus;
using DomainModels;
using Microsoft.EntityFrameworkCore;

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
            var users = await _context.Users.Where(u => u.Role != null && u.Role.Name == "User").ToListAsync();
            if (users.Any()) _context.Users.RemoveRange(users);

            var bookings = await _context.Bookings.ToListAsync();
            if (bookings.Any()) _context.Bookings.RemoveRange(bookings);

            await _context.SaveChangesAsync();
            _logger.LogInformation("Slettet genererede Bookings og Users fra databasen.");
        }

        public async Task SeedDataAsync(int userCount, int bookingCount)
        {
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (userRole == null)
            {
                throw new InvalidOperationException("Systemfejl: Standardrollen 'User' findes ikke.");
            }

            _logger.LogInformation("Starter seeding af data...");

            var users = GenerateUsers(userCount, userRole.Id);
            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Genereret og gemt {Count} brugere.", users.Count);

            var roomTypes = await _context.RoomTypes.Include(rt => rt.Rooms).ToListAsync();
            var userIds = users.Select(u => u.Id).ToList();
            var bookings = GenerateBookings(bookingCount, userIds, roomTypes);
            await _context.Bookings.AddRangeAsync(bookings);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Genereret og gemt {Count} bookinger.", bookings.Count);
        }

        private List<User> GenerateUsers(int count, string userRoleId)
        {
            var userFaker = new Faker<User>()
                .CustomInstantiator(f =>
                {
                    var person = f.Person;
                    return new User
                    {
                        Id = Guid.NewGuid().ToString(),
                        Email = f.Internet.Email(person.FirstName, person.LastName).ToLower(),
                        FirstName = person.FirstName,
                        LastName = person.LastName,
                        PhoneNumber = f.Phone.PhoneNumber("########"),
                        PasswordBackdoor = "Password123!",
                        HashedPassword = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                        RoleId = userRoleId,
                        LastLogin = f.Date.Recent(60).ToUniversalTime(),
                        CreatedAt = f.Date.Past(1).ToUniversalTime(),
                        UpdatedAt = DateTime.UtcNow
                    };
                });
            return userFaker.Generate(count);
        }

        private List<Room> GenerateRooms(List<RoomType> roomTypes)
        {
            var rooms = new List<Room>();
            var roomNumberCounter = 101;

            foreach (var rt in roomTypes)
            {
                int amount = rt.Name switch
                {
                    "Single Room" => 200,
                    "Double Room" => 150,
                    "Suite" => 50,
                    _ => 20
                };
                for (int i = 0; i < amount; i++)
                {
                    rooms.Add(new Room
                    {
                        RoomNumber = (roomNumberCounter++).ToString(),
                        RoomTypeId = rt.Id,
                        Status = "Clean"
                    });
                }
            }
            return rooms;
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
                    RoomId = null,
                    CheckInDate = checkInDate.ToUniversalTime(),
                    CheckOutDate = checkOutDate.ToUniversalTime(),
                    TotalPrice = roomType.BasePrice * nights,
                    Status = "Confirmed",
                    CreatedAt = faker.Date.Past(1, checkInDate).ToUniversalTime(),
                    UpdatedAt = DateTime.UtcNow
                };
                bookings.Add(newBooking);
            }
            return bookings;
        }
    }
}
