using DomainModels;
using Microsoft.EntityFrameworkCore;
using System;

namespace API.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options)
            : base(options)
        {
        }

        // DbSets for alle dine entiteter
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Room> Rooms { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Konfiguration for Role
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasIndex(r => r.Name).IsUnique();
            });

            // Konfiguration for User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasOne(u => u.Role)
                      .WithMany(r => r.Users)
                      .HasForeignKey(u => u.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Konfiguration for Room
            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasIndex(r => r.RoomNumber).IsUnique();
                entity.Property(r => r.PricePerNight).HasColumnType("decimal(18,2)");
            });

            // Konfiguration for Booking
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasOne(b => b.User)
                      .WithMany()
                      .HasForeignKey(b => b.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.Room)
                      .WithMany(r => r.Bookings)
                      .HasForeignKey(b => b.RoomId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(b => b.TotalPrice).HasColumnType("decimal(18,2)");
            });

            // Seed data for roller og værelser
            SeedRoles(modelBuilder);
            SeedRooms(modelBuilder);
        }

        private void SeedRoles(ModelBuilder modelBuilder)
        {
            var roles = new[]
            {
                new Role { Id = "1", Name = "User", Description = "Standard bruger med basis rettigheder", CreatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc) },
                new Role { Id = "2", Name = "CleaningStaff", Description = "Rengøringspersonale med adgang til rengøringsmoduler", CreatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc) },
                new Role { Id = "3", Name = "Reception", Description = "Receptionspersonale med adgang til booking og gæster", CreatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc) },
                new Role { Id = "4", Name = "Admin", Description = "Administrator med fuld adgang til systemet", CreatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc) }
            };
            modelBuilder.Entity<Role>().HasData(roles);
        }

        // Rettet SeedRooms-metode med statiske værdier
        private void SeedRooms(ModelBuilder modelBuilder)
        {
            var seedDate = new DateTime(2025, 8, 14, 10, 0, 0, DateTimeKind.Utc);

            var rooms = new[]
            {
                new Room { Id = "c7e2c9f8-4a5f-4a6a-8f0c-5f8d7f6e5e4d", RoomNumber = "101", Type = "Single", PricePerNight = 800m, Description = "Hyggeligt enkeltværelse med udsigt over gården.", CreatedAt = seedDate, UpdatedAt = seedDate },
                new Room { Id = "d8e3d0a9-5b6g-5b7b-9g1d-6g9e8g7f6f5e", RoomNumber = "102", Type = "Single", PricePerNight = 800m, Description = "Hyggeligt enkeltværelse med udsigt over gården.", CreatedAt = seedDate, UpdatedAt = seedDate },
                new Room { Id = "e9f4e1b0-6c7h-6c8c-0h2e-7h0f9h8g7g6f", RoomNumber = "201", Type = "Double", PricePerNight = 1200m, Description = "Rummeligt dobbeltværelse med balkon.", CreatedAt = seedDate, UpdatedAt = seedDate },
                new Room { Id = "f0g5f2c1-7d8i-7d9d-1i3f-8i1g0i9h8h7g", RoomNumber = "202", Type = "Double", PricePerNight = 1200m, Description = "Rummeligt dobbeltværelse med balkon.", CreatedAt = seedDate, UpdatedAt = seedDate },
                new Room { Id = "g1h6g3d2-8e9j-8e0e-2j4g-9j2h1j0i9i8h", RoomNumber = "301", Type = "Suite", PricePerNight = 2500m, Description = "Luksuriøs suite med separat stue og havudsigt.", CreatedAt = seedDate, UpdatedAt = seedDate }
            };
            modelBuilder.Entity<Room>().HasData(rooms);
        }
    }
}