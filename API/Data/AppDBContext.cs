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

        // Bemærk: DbSet for BookingService er fjernet, da EF Core nu håndterer det.

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is Common && (
                        e.State == EntityState.Added
                        || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                ((Common)entityEntry.Entity).UpdatedAt = DateTime.UtcNow;

                if (entityEntry.State == EntityState.Added)
                {
                    ((Common)entityEntry.Entity).CreatedAt = DateTime.UtcNow;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>(entity => entity.HasIndex(r => r.Name).IsUnique());

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasOne(u => u.Role).WithMany(r => r.Users).HasForeignKey(u => u.RoleId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasIndex(r => r.RoomNumber).IsUnique();
                entity.HasOne(r => r.RoomType).WithMany(rt => rt.Rooms).HasForeignKey(r => r.RoomTypeId);
            });

            modelBuilder.Entity<RoomType>(entity =>
            {
                entity.Property(rt => rt.BasePrice).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<Booking>(entity =>
            {
                entity.Property(b => b.TotalPrice).HasColumnType("decimal(18,2)");
                entity.HasOne(b => b.User).WithMany().HasForeignKey(b => b.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(b => b.RoomType).WithMany().HasForeignKey(b => b.RoomTypeId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(b => b.Room).WithMany(r => r.Bookings).HasForeignKey(b => b.RoomId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);

                // Ny, automatisk måde at definere mange-til-mange-relationen på
                entity.HasMany(b => b.Services)
                      .WithMany(s => s.Bookings)
                      .UsingEntity("BookingServices");
            });

            modelBuilder.Entity<Service>(entity =>
            {
                entity.Property(s => s.Price).HasColumnType("decimal(18,2)");
            });

            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            var roles = new[]
            {
                new Role { Id = "1", Name = "User", Description = "Standard bruger" },
                new Role { Id = "2", Name = "Housekeeping", Description = "Rengøringspersonale" },
                new Role { Id = "3", Name = "Receptionist", Description = "Receptionspersonale" },
                new Role { Id = "4", Name = "Manager", Description = "Hotel Manager" }
            };
            modelBuilder.Entity<Role>().HasData(roles);

            var passwordHash = "$2a$11$jCvV3t1G2u2AL.26A72Gv.ECi1G93olRzSP4i3.eIh3Kx/p2yvD.W"; // Hash for "Password123!"
            var now = DateTime.UtcNow;

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = Guid.NewGuid().ToString(),
                    FirstName = "Manager",
                    LastName = "Admin",
                    Email = "manager@hotel.dk",
                    RoleId = "4",
                    HashedPassword = passwordHash,
                    PasswordBackdoor = "Password123!",
                    CreatedAt = now,
                    UpdatedAt = now,
                    LastLogin = now
                },
                new User
                {
                    Id = Guid.NewGuid().ToString(),
                    FirstName = "Receptionist",
                    LastName = "Test",
                    Email = "receptionist@hotel.dk",
                    RoleId = "3",
                    HashedPassword = passwordHash,
                    PasswordBackdoor = "Password123!",
                    CreatedAt = now,
                    UpdatedAt = now,
                    LastLogin = now
                },
                new User
                {
                    Id = Guid.NewGuid().ToString(),
                    FirstName = "Rengøring",
                    LastName = "Test",
                    Email = "rengøring@hotel.dk",
                    RoleId = "2",
                    HashedPassword = passwordHash,
                    PasswordBackdoor = "Password123!",
                    CreatedAt = now,
                    UpdatedAt = now,
                    LastLogin = now
                }
            );

            var roomTypes = new[]
            {
                new RoomType { Id = 1, Name = "Single Room", Description = "Hyggeligt enkeltværelse med alt hvad du behøver.", BasePrice = 800m, Capacity = 1 },
                new RoomType { Id = 2, Name = "Double Room", Description = "Rummeligt dobbeltværelse med plads til to.", BasePrice = 1200m, Capacity = 2 },
                new RoomType { Id = 3, Name = "Suite", Description = "Luksuriøs suite med separat opholdsområde og fantastisk udsigt.", BasePrice = 2500m, Capacity = 4 }
            };
            modelBuilder.Entity<RoomType>().HasData(roomTypes);

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