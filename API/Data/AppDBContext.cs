using DomainModels;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Room> Rooms { get; set; } = null!;
        public DbSet<RoomType> RoomTypes { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;
        public DbSet<Service> Services { get; set; } = null!;
        public DbSet<BookingService> BookingServices { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User & Role konfiguration (som før)
            modelBuilder.Entity<Role>(entity => entity.HasIndex(r => r.Name).IsUnique());
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasOne(u => u.Role).WithMany(r => r.Users).HasForeignKey(u => u.RoleId).OnDelete(DeleteBehavior.Restrict);
            });

            // Room & RoomType konfiguration
            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasIndex(r => r.RoomNumber).IsUnique();
                entity.HasOne(r => r.RoomType).WithMany(rt => rt.Rooms).HasForeignKey(r => r.RoomTypeId);
            });
            modelBuilder.Entity<RoomType>(entity =>
            {
                entity.Property(rt => rt.BasePrice).HasColumnType("decimal(18,2)");
            });

            // Booking konfiguration
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.Property(b => b.TotalPrice).HasColumnType("decimal(18,2)");
                entity.HasOne(b => b.User).WithMany().HasForeignKey(b => b.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(b => b.RoomType).WithMany().HasForeignKey(b => b.RoomTypeId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(b => b.Room).WithMany(r => r.Bookings).HasForeignKey(b => b.RoomId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
            });

            // Service & BookingService konfiguration (mange-til-mange)
            modelBuilder.Entity<Service>(entity =>
            {
                entity.Property(s => s.Price).HasColumnType("decimal(18,2)");
            });
            modelBuilder.Entity<BookingService>(entity =>
            {
                entity.HasKey(bs => new { bs.BookingId, bs.ServiceId });
                entity.HasOne(bs => bs.Booking).WithMany(b => b.BookingServices).HasForeignKey(bs => bs.BookingId);
                entity.HasOne(bs => bs.Service).WithMany().HasForeignKey(bs => bs.ServiceId);
            });

            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Roller (er korrekt)
            var roles = new[]
            {
        new Role { Id = "1", Name = "User", Description = "Standard bruger" },
        new Role { Id = "2", Name = "Housekeeping", Description = "Rengøringspersonale" },
        new Role { Id = "3", Name = "Receptionist", Description = "Receptionspersonale" },
        new Role { Id = "4", Name = "Manager", Description = "Hotel Manager" }
    };
            modelBuilder.Entity<Role>().HasData(roles);

            // Seed Værelsestyper (er korrekt)
            var roomTypes = new[]
            {
        new RoomType { Id = 1, Name = "Single Room", Description = "Hyggeligt enkeltværelse med alt hvad du behøver.", BasePrice = 800m, Capacity = 1 },
        new RoomType { Id = 2, Name = "Double Room", Description = "Rummeligt dobbeltværelse med plads til to.", BasePrice = 1200m, Capacity = 2 },
        new RoomType { Id = 3, Name = "Suite", Description = "Luksuriøs suite med separat opholdsområde og fantastisk udsigt.", BasePrice = 2500m, Capacity = 4 }
    };
            modelBuilder.Entity<RoomType>().HasData(roomTypes);

            // ---> ÆNDRING START: Opdateret antal værelser <---
            // Seed Værelser (nu med 400 i alt)
            var rooms = new List<Room>();
            int roomIdCounter = 1;

            // 150 Single Rooms
            for (int i = 0; i < 150; i++)
            {
                rooms.Add(new Room { Id = roomIdCounter++, RoomNumber = (100 + i).ToString(), RoomTypeId = 1, Status = "Clean" });
            }
            // 200 Double Rooms
            for (int i = 0; i < 200; i++)
            {
                rooms.Add(new Room { Id = roomIdCounter++, RoomNumber = (300 + i).ToString(), RoomTypeId = 2, Status = "Clean" });
            }
            // 50 Suites
            for (int i = 0; i < 50; i++)
            {
                rooms.Add(new Room { Id = roomIdCounter++, RoomNumber = (500 + i).ToString(), RoomTypeId = 3, Status = "Clean" });
            }
            modelBuilder.Entity<Room>().HasData(rooms);

            var services = new[]
            {
        new Service { Id = 1, Name = "Breakfast", Price = 150m, BillingType = "PerNight" },
        new Service { Id = 2, Name = "Spa Access", Price = 250m, BillingType = "PerBooking" },
        new Service { Id = 3, Name = "Champagne on arrival", Price = 400m, BillingType = "PerBooking" }
             };
            modelBuilder.Entity<Service>().HasData(services);
        }
    }
}