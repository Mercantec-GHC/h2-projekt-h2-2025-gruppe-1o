using API.Data;
using Bogus;
using DomainModels;
using Microsoft.EntityFrameworkCore;
// RETTET: Den fulde sti er BCrypt.Net.BCrypt
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
            // RETTET (ADVARSEL): Tilføjet 'u.Role != null' for at undgå null-reference advarsel.
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
            // Tjek om der allerede ER værelser, så vi ikke tilføjer 400 oveni.
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

            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (userRole == null)
            {
                _logger.LogError("Kan ikke seede data: Standardrollen 'User' findes ikke.");
                return;
            }

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
            var userFaker = new Faker<User>("da")
                .RuleFor(u => u.Id, f => Guid.NewGuid().ToString())
                .RuleFor(u => u.Email, f => f.Internet.Email(f.Person.FirstName, f.Person.LastName).ToLower())
                .RuleFor(u => u.Username, (f, u) => f.Internet.UserName(f.Person.FirstName, f.Person.LastName))
                .RuleFor(u => u.PasswordBackdoor, "Password123!")
                .RuleFor(u => u.HashedPassword, FullBCrypt.HashPassword("Password123!"))
                .RuleFor(u => u.RoleId, userRoleId)
                .RuleFor(u => u.LastLogin, f => f.Date.Recent(60))
                .RuleFor(u => u.CreatedAt, f => f.Date.Past(1))
                .RuleFor(u => u.UpdatedAt, (f, u) => u.CreatedAt);

            return userFaker.Generate(count);
        }

        // Indsæt denne metode i API/Services/DataSeederService.cs
        private List<Room> GenerateRooms(List<RoomType> roomTypes)
        {
            var rooms = new List<Room>();
            var roomNumberCounter = 100;

            // 200 Single Rooms
            for (int i = 0; i < 200; i++)
            {
                rooms.Add(new Room { RoomNumber = (roomNumberCounter++).ToString(), RoomTypeId = 1, Status = "Clean" });
            }

            // 150 Double Rooms
            for (int i = 0; i < 150; i++)
            {
                rooms.Add(new Room { RoomNumber = (roomNumberCounter++).ToString(), RoomTypeId = 2, Status = "Clean" });
            }

            // 50 Suites
            for (int i = 0; i < 50; i++)
            {
                rooms.Add(new Room { RoomNumber = (roomNumberCounter++).ToString(), RoomTypeId = 3, Status = "Clean" });
            }
            _logger.LogInformation("Genereret {Count} værelser til seeding.", rooms.Count);
            return rooms;
        }

        private List<Booking> GenerateBookings(int count, List<string> userIds, List<RoomType> roomTypes)
        {
            var occupancy = new Dictionary<int, Dictionary<DateOnly, int>>();
            foreach (var rt in roomTypes)
            {
                occupancy[rt.Id] = new Dictionary<DateOnly, int>();
            }

            var bookings = new List<Booking>();
            var faker = new Faker();

            for (int i = 0; i < count; i++)
            {
                var roomType = faker.PickRandom(roomTypes);
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

                    var newBooking = new Faker<Booking>("da")
                        .RuleFor(b => b.Id, f => Guid.NewGuid().ToString())
                        .RuleFor(b => b.UserId, f => f.PickRandom(userIds))
                        .RuleFor(b => b.RoomTypeId, roomType.Id)
                        .RuleFor(b => b.RoomId, (int?)null)
                        .RuleFor(b => b.CheckInDate, checkInDate.ToUniversalTime())
                        .RuleFor(b => b.CheckOutDate, checkOutDate.ToUniversalTime())
                        .RuleFor(b => b.TotalPrice, roomType.BasePrice * nights)
                        .RuleFor(b => b.Status, "Confirmed")
                        .RuleFor(b => b.CreatedAt, (f, b) => f.Date.Past(1, b.CheckInDate))
                        .Generate();

                    newBooking.UpdatedAt = newBooking.CreatedAt;
                    bookings.Add(newBooking);
                }
            }
            return bookings;
        }
    }
}