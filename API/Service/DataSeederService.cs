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
            var users = await _context.Users.Where(u => u.Role != null && u.Role.Name == "User").ToListAsync();
            if (users.Any())
            {
                _context.Users.RemoveRange(users);
            }

            var bookings = await _context.Bookings.ToListAsync();
            if (bookings.Any())
            {
                _context.Bookings.RemoveRange(bookings);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Slettet genererede Bookings og Users fra databasen.");
        }

        public async Task SeedDataAsync(int userCount, int bookingCount)
        {
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (userRole == null)
            {
                string errorMessage = "Kan ikke seede data: Standardrollen 'User' findes ikke. Sørg for at databasen er migreret korrekt.";
                _logger.LogCritical(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            _logger.LogInformation("Starter seeding af data...");

            // Rooms
            if (await _context.Rooms.AnyAsync())
            {
                _logger.LogInformation("Databasen indeholder allerede værelser. Skipper værelses-seeding.");
            }
            else
            {
                var roomTypesForSeeding = await _context.RoomTypes.ToListAsync();
                var rooms = GenerateRooms(roomTypesForSeeding);
                await _context.Rooms.AddRangeAsync(rooms);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Gemt {Count} nye værelser i databasen.", rooms.Count);
            }

            // Users
            var users = GenerateUsers(userCount, userRole.Id);
            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Genereret og gemt {Count} brugere.", users.Count);

            // Bookings
            var roomTypes = await _context.RoomTypes.Include(rt => rt.Rooms).ToListAsync();
            var userIds = users.Select(u => u.Id).ToList();
            var bookings = GenerateBookings(bookingCount, userIds, roomTypes);
            await _context.Bookings.AddRangeAsync(bookings);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Genereret og gemt {Count} bookinger.", bookings.Count);
        }

        // === USERS ===
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
                        Username = f.Internet.UserName(person.FirstName, person.LastName),
                        PasswordBackdoor = "Password123!",
                        HashedPassword = FullBCrypt.HashPassword("Password123!"),
                        RoleId = userRoleId,
                        LastLogin = f.Date.Recent(60),
                        CreatedAt = f.Date.Past(1),
                        UpdatedAt = DateTime.UtcNow
                    };
                });

            return userFaker.Generate(count);
        }

        // === ROOMS ===
        private List<Room> GenerateRooms(List<RoomType> roomTypes)
        {
            var rooms = new List<Room>();
            var roomNumberCounter = 101;

            foreach (var rt in roomTypes)
            {
                int amount = rt.Name switch
                {
                    "Single" => 200,
                    "Double" => 150,
                    "Suite" => 50,
                    _ => 20 // fallback, hvis der er andre typer
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

            _logger.LogInformation("Genereret {Count} værelser til seeding.", rooms.Count);
            return rooms;
        }

        // === BOOKINGS ===
        private List<Booking> GenerateBookings(int count, List<string> userIds, List<RoomType> roomTypes)
        {
            var occupancy = new Dictionary<int, Dictionary<DateOnly, int>>();
            foreach (var rt in roomTypes)
            {
                occupancy[rt.Id] = new Dictionary<DateOnly, int>();
            }

            var bookings = new List<Booking>();
            var faker = new Faker();
            int createdBookings = 0;

            while (createdBookings < count && userIds.Any())
            {
                var roomType = faker.PickRandom(roomTypes);
                if (!roomType.Rooms.Any()) continue;

                var nights = faker.Random.Int(1, 7);
                var checkInDate = faker.Date.Future(1).Date;
                var checkOutDate = checkInDate.AddDays(nights);

                bool isAvailable = true;
                for (var date = checkInDate; date < checkOutDate; date = date.AddDays(1))
                {
                    var dateOnly = DateOnly.FromDateTime(date);
                    occupancy[roomType.Id].TryGetValue(dateOnly, out int bookedCount);
                    if (bookedCount >= roomType.Rooms.Count)
                    {
                        isAvailable = false;
                        break;
                    }
                }

                if (isAvailable)
                {
                    for (var date = checkInDate; date < checkOutDate; date = date.AddDays(1))
                    {
                        var dateOnly = DateOnly.FromDateTime(date);
                        if (!occupancy[roomType.Id].ContainsKey(dateOnly))
                        {
                            occupancy[roomType.Id][dateOnly] = 0;
                        }
                        occupancy[roomType.Id][dateOnly]++;
                    }

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
                        CreatedAt = faker.Date.Past(1, checkInDate),
                        UpdatedAt = DateTime.UtcNow
                    };

                    bookings.Add(newBooking);
                    createdBookings++;
                }
            }
            return bookings;
        }
    }
}