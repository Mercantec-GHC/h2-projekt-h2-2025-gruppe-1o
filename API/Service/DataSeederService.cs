using API.Data;
using Bogus;
using DomainModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            if (await _context.Bookings.CountAsync() > 100)
            {
                _logger.LogInformation("Database already contains a significant number of bookings. Skipping data seed.");
                return;
            }

            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (userRole == null)
            {
                throw new InvalidOperationException("Systemfejl: Standardrollen 'User' findes ikke.");
            }

            _logger.LogInformation("Starter seeding af {userCount} brugere...", userCount);
            var users = GenerateUsers(userCount, userRole.Id);
            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Genereret og gemt {Count} brugere.", users.Count);

            _logger.LogInformation("Starter seeding af ca. {bookingCount} bookinger...", bookingCount);
            var roomTypes = await _context.RoomTypes.Include(rt => rt.Rooms).ToListAsync();
            var userIds = users.Select(u => u.Id).ToList();
            var bookings = GenerateBookingsWithRooms(bookingCount, userIds, roomTypes);
            await _context.Bookings.AddRangeAsync(bookings);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Genereret og gemt {Count} bookinger med tildelte rum.", bookings.Count);
        }

        private List<User> GenerateUsers(int count, string userRoleId)
        {
            var userFaker = new Faker<User>()
                .RuleFor(u => u.Id, f => Guid.NewGuid().ToString())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName).ToLower())
                .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber("########"))
                .RuleFor(u => u.HashedPassword, BCrypt.Net.BCrypt.HashPassword("Password123!"))
                .RuleFor(u => u.RoleId, userRoleId)
                .RuleFor(u => u.CreatedAt, f => f.Date.Past(2).ToUniversalTime())
                .RuleFor(u => u.UpdatedAt, f => f.Date.Recent().ToUniversalTime())
                // RETTELSEN ER HER: LastLogin manglede
                .RuleFor(u => u.LastLogin, f => f.Date.Past(1).ToUniversalTime());

            return userFaker.Generate(count);
        }

        private List<Booking> GenerateBookingsWithRooms(int count, List<string> userIds, List<RoomType> roomTypes)
        {
            var bookings = new List<Booking>();
            var faker = new Faker();

            var roomOccupation = new Dictionary<int, List<(DateTime, DateTime)>>();

            for (int i = 0; i < count; i++)
            {
                var roomType = faker.PickRandom(roomTypes);
                if (!roomType.Rooms.Any()) continue;

                var nights = faker.Random.Int(1, 14);
                var checkInDate = faker.Date.Between(DateTime.UtcNow.AddMonths(-6), DateTime.UtcNow.AddMonths(6)).Date;
                var checkOutDate = checkInDate.AddDays(nights);

                Room? availableRoom = null;
                foreach (var room in roomType.Rooms.OrderBy(r => Guid.NewGuid()))
                {
                    if (!roomOccupation.TryGetValue(room.Id, out var occupiedPeriods))
                    {
                        availableRoom = room;
                        break;
                    }

                    var isOccupied = occupiedPeriods.Any(period => checkInDate < period.Item2 && checkOutDate > period.Item1);
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
                        roomOccupation[availableRoom.Id] = new List<(DateTime, DateTime)>();
                    }
                    roomOccupation[availableRoom.Id].Add((checkInDate, checkOutDate));
                }
            }
            return bookings;
        }
    }
}