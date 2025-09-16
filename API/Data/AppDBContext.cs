using DomainModels;
using DomainModels.Enums; // Husk at tilføje denne using-statement
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

            // Din eksisterende model-konfiguration er fin og forbliver uændret
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
            // Din Role, User og RoomType seeding forbliver uændret
            var roles = new[] { new Role { Id = "1", Name = "User", Description = "Standard bruger" }, new Role { Id = "2", Name = "Housekeeping", Description = "Rengøringspersonale" }, new Role { Id = "3", Name = "Receptionist", Description = "Receptionspersonale" }, new Role { Id = "4", Name = "Manager", Description = "Hotel Manager" } };
            modelBuilder.Entity<Role>().HasData(roles);

            // ... dine users ...

            var roomTypes = new[] { new RoomType { Id = 1, Name = "Single Room", Description = "Hyggeligt enkeltværelse med alt hvad du behøver.", BasePrice = 800m, Capacity = 1 }, new RoomType { Id = 2, Name = "Double Room", Description = "Rummeligt dobbeltværelse med plads til to.", BasePrice = 1200m, Capacity = 2 }, new RoomType { Id = 3, Name = "Suite", Description = "Luksuriøs suite med separat opholdsområde og fantastisk udsigt.", BasePrice = 2500m, Capacity = 4 } };
            modelBuilder.Entity<RoomType>().HasData(roomTypes);

            // Udskiftning af din gamle service-liste med den nye
            modelBuilder.Entity<Service>().HasData(
                // Mad & Drikke
                new Service { Id = 1, Name = "Morgenmad på værelset", Description = "Lækker morgenmad serveret direkte på dit værelse.", Price = 150.00m, BillingType = BillingType.PerPersonPerNight, Category = "Mad & Drikke", IsActive = true },
                new Service { Id = 2, Name = "Champagne ved ankomst", Description = "En afkølet flaske Moët & Chandon venter på værelset.", Price = 400.00m, BillingType = BillingType.PerBooking, Category = "Mad & Drikke", IsActive = true },
                new Service { Id = 3, Name = "Luksus Frugtkurv", Description = "Et udvalg af sæsonens friske, eksotiske frugter.", Price = 120.00m, BillingType = BillingType.PerBooking, Category = "Mad & Drikke", IsActive = true },
                new Service { Id = 4, Name = "Chokolade & Macarons", Description = "Håndlavet luksuschokolade og franske macarons.", Price = 95.00m, BillingType = BillingType.PerBooking, Category = "Mad & Drikke", IsActive = true },

                // Wellness & Afslapning
                new Service { Id = 5, Name = "Spa Adgang", Description = "Fuld adgang til vores luksuriøse spa- og wellnessområde.", Price = 250.00m, BillingType = BillingType.PerPerson, Category = "Wellness & Afslapning", IsActive = true },
                new Service { Id = 6, Name = "60 min. Par-massage", Description = "Afslappende massage for to personer i vores private suite.", Price = 1200.00m, BillingType = BillingType.PerBooking, Category = "Wellness & Afslapning", IsActive = true },

                // Praktisk & Komfort
                new Service { Id = 7, Name = "Sen Udtjekning (kl. 14:00)", Description = "Sov lidt længere og nyd værelset i et par ekstra timer.", Price = 200.00m, BillingType = BillingType.PerBooking, Category = "Praktisk & Komfort", IsActive = true },
                new Service { Id = 8, Name = "Sikker Parkering", Description = "Garanteret parkeringsplads i vores overvågede kælder.", Price = 150.00m, BillingType = BillingType.PerNight, Category = "Praktisk & Komfort", IsActive = true },
                new Service { Id = 9, Name = "Lufthavnstransfer", Description = "Privat luksusbil til eller fra lufthavnen.", Price = 600.00m, BillingType = BillingType.PerBooking, Category = "Praktisk & Komfort", IsActive = true },

                // Særlige Lejligheder
                new Service { Id = 10, Name = "Romantisk Pakke", Description = "Rosenblade på sengen, stearinlys og en flaske Cava.", Price = 450.00m, BillingType = BillingType.PerBooking, Category = "Særlige Lejligheder", IsActive = true },
                new Service { Id = 11, Name = "Friske Blomster på værelset", Description = "En smuk, frisk buket fra vores lokale florist.", Price = 250.00m, BillingType = BillingType.PerBooking, Category = "Særlige Lejligheder", IsActive = true }
            );
        }
    }
}