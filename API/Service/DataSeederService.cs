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
            if (await _context.Bookings.AnyAsync())
            {
                _logger.LogInformation("Database already contains bookings. Skipping data seed.");
                return;
            }

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

            // RETTELSEN ER HER: Vi kalder nu den korrekte metode
            var bookings = GenerateBookingsWithRooms(bookingCount, userIds, roomTypes);

            await _context.Bookings.AddRangeAsync(bookings);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Genereret og gemt {Count} bookinger med tildelte rum.", bookings.Count);
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
                        HashedPassword = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                        RoleId = userRoleId,
                        LastLogin = f.Date.Recent(60).ToUniversalTime(),
                        CreatedAt = f.Date.Past(1).ToUniversalTime(),
                        UpdatedAt = DateTime.UtcNow
                    };
                });
            return userFaker.Generate(count);
        }

        private List<Booking> GenerateBookingsWithRooms(int count, List<string> userIds, List<RoomType> roomTypes)
        {
            var bookings = new List<Booking>();
            var faker = new Faker();
            var roomOccupation = new Dictionary<int, List<(DateTime Start, DateTime End)>>();

            for (int i = 0; i < count; i++)
            {
                var roomType = faker.PickRandom(roomTypes);
                if (!roomType.Rooms.Any()) continue;

                var nights = faker.Random.Int(1, 7);
                var checkInDate = faker.Date.Future(1, DateTime.UtcNow.AddMonths(6)).Date;
                var checkOutDate = checkInDate.AddDays(nights);

                Room? availableRoom = null;
                foreach (var room in roomType.Rooms.OrderBy(r => Guid.NewGuid()))
                {
                    if (!roomOccupation.ContainsKey(room.Id))
                    {
                        availableRoom = room;
                        break;
                    }

                    var isOccupied = roomOccupation[room.Id]
                        .Any(period => checkInDate < period.End && checkOutDate > period.Start);

                    if (!isOccupied)
                    {
                        availableRoom = room;
                        break;
                    }
                }

                if (availableRoom != null)
                {
                    var newBooking = new Booking
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = faker.PickRandom(userIds),
                        RoomTypeId = roomType.Id,
                        RoomId = availableRoom.Id,
                        CheckInDate = checkInDate.ToUniversalTime(),
                        CheckOutDate = checkOutDate.ToUniversalTime(),
                        TotalPrice = roomType.BasePrice * nights,
                        Status = "Confirmed",
                        CreatedAt = faker.Date.Past(1, checkInDate).ToUniversalTime(),
                        UpdatedAt = DateTime.UtcNow
                    };
                    bookings.Add(newBooking);

                    if (!roomOccupation.ContainsKey(availableRoom.Id))
                    {
                        roomOccupation[availableRoom.Id] = new List<(DateTime Start, DateTime End)>();
                    }
                    roomOccupation[availableRoom.Id].Add((checkInDate, checkOutDate));
                }
            }
            return bookings;
        }
    }
}