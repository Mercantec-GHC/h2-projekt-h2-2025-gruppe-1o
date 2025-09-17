using DomainModels;
using DomainModels.Enums;
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

            // Eksisterende konfigurationer...
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
                // NYT: Vi genindfører relationen til RoomType for at definere TILGÆNGELIGHED
                entity.HasMany(s => s.RoomTypes)
                      .WithMany(rt => rt.Services)
                      .UsingEntity(j => j.ToTable("RoomTypeServices"));
            });

            SeedStaticData(modelBuilder);
        }

        private void SeedStaticData(ModelBuilder modelBuilder)
        {
            // Seeding af Roller, RoomTypes, og Rooms er uændret...
            var roles = new[] { new Role { Id = "1", Name = "User", Description = "Standard bruger" }, new Role { Id = "2", Name = "Housekeeping", Description = "Rengøringspersonale" }, new Role { Id = "3", Name = "Receptionist", Description = "Receptionspersonale" }, new Role { Id = "4", Name = "Manager", Description = "Hotel Manager" }, new Role { Id = "5", Name = "Admin", Description = "System Administrator" } };
            modelBuilder.Entity<Role>().HasData(roles);
            var roomTypes = new[] { new RoomType { Id = 1, Name = "Standard Værelse", Description = "Hyggeligt enkeltværelse med alt hvad du behøver.", BasePrice = 800m, Capacity = 2 }, new RoomType { Id = 2, Name = "Deluxe Suite", Description = "Rummelig suite med panoramaudsigt og premium faciliteter.", BasePrice = 2200m, Capacity = 4 }, new RoomType { Id = 3, Name = "Presidential Suite", Description = "Den ultimative luksusoplevelse med eksklusiv service.", BasePrice = 5000m, Capacity = 8 } };
            modelBuilder.Entity<RoomType>().HasData(roomTypes);
            var rooms = new List<Room>();
            var roomCounter = 1;
            for (int i = 0; i < 20; i++) rooms.Add(new Room { Id = roomCounter++, RoomNumber = (101 + i).ToString(), Status = "Clean", RoomTypeId = 1 });
            for (int i = 0; i < 20; i++) rooms.Add(new Room { Id = roomCounter++, RoomNumber = (201 + i).ToString(), Status = "Clean", RoomTypeId = 2 });
            for (int i = 0; i < 10; i++) rooms.Add(new Room { Id = roomCounter++, RoomNumber = (301 + i).ToString(), Status = "Clean", RoomTypeId = 3 });
            modelBuilder.Entity<Room>().HasData(rooms);

            // Master-listen af 24 services er uændret
            modelBuilder.Entity<Service>().HasData(
                new Service { Id = 1, Name = "Morgenmad på værelset", Description = "Lækker morgenmad serveret direkte på dit værelse.", Price = 150.00m, BillingType = BillingType.PerPersonPerNight, Category = "Mad & Drikke", IsActive = true },
                new Service { Id = 2, Name = "Champagne ved ankomst", Description = "En afkølet flaske Moët & Chandon venter på værelset.", Price = 400.00m, BillingType = BillingType.PerBooking, Category = "Mad & Drikke", IsActive = true },
                new Service { Id = 3, Name = "Luksus Frugtkurv", Description = "Et udvalg af sæsonens friske, eksotiske frugter.", Price = 120.00m, BillingType = BillingType.PerBooking, Category = "Mad & Drikke", IsActive = true },
                new Service { Id = 4, Name = "Chokolade & Macarons", Description = "Håndlavet luksuschokolade og franske macarons.", Price = 95.00m, BillingType = BillingType.PerBooking, Category = "Mad & Drikke", IsActive = true },
                new Service { Id = 5, Name = "Gin & Tonic Kit", Description = "Premium Gin og tonic-vand med garniture.", Price = 180.00m, BillingType = BillingType.PerBooking, Category = "Mad & Drikke", IsActive = true },
                new Service { Id = 6, Name = "Sushi Menu", Description = "Udsøgt sushi fra hotellets restaurant, leveret til dit værelse.", Price = 350.00m, BillingType = BillingType.PerPerson, Category = "Mad & Drikke", IsActive = true },
                new Service { Id = 7, Name = "Vin & Ostesmagning", Description = "En kurateret smagsoplevelse med udsøgte vine og oste.", Price = 500.00m, BillingType = BillingType.PerBooking, Category = "Mad & Drikke", IsActive = true },
                new Service { Id = 8, Name = "Late Night Snacks", Description = "Et udvalg af salte og søde snacks.", Price = 85.00m, BillingType = BillingType.PerBooking, Category = "Mad & Drikke", IsActive = true },
                new Service { Id = 9, Name = "Spa Adgang", Description = "Fuld dagsadgang til vores luksuriøse spa- og wellnessområde.", Price = 250.00m, BillingType = BillingType.PerPerson, Category = "Wellness & Afslapning", IsActive = true },
                new Service { Id = 10, Name = "60 min. Par-massage", Description = "Afslappende massage for to i vores private suite.", Price = 1200.00m, BillingType = BillingType.PerBooking, Category = "Wellness & Afslapning", IsActive = true },
                new Service { Id = 11, Name = "Yogamatte og instruktion", Description = "Få en yogamatte og en guide til morgen-yoga.", Price = 50.00m, BillingType = BillingType.PerBooking, Category = "Wellness & Afslapning", IsActive = true },
                new Service { Id = 12, Name = "Personlig Træner", Description = "En privat træningssession med en certificeret træner.", Price = 450.00m, BillingType = BillingType.PerBooking, Category = "Wellness & Afslapning", IsActive = true },
                new Service { Id = 13, Name = "Privat Sauna Session", Description = "Book vores private sauna til en times eksklusiv brug.", Price = 300.00m, BillingType = BillingType.PerBooking, Category = "Wellness & Afslapning", IsActive = true },
                new Service { Id = 14, Name = "Badekåbe & Sutsko", Description = "Hjemmesko og en luksuriøs badekåbe til at tage med hjem.", Price = 180.00m, BillingType = BillingType.PerPerson, Category = "Wellness & Afslapning", IsActive = true },
                new Service { Id = 15, Name = "Sen Udtjekning (kl. 14:00)", Description = "Sov lidt længere og nyd værelset i et par ekstra timer.", Price = 200.00m, BillingType = BillingType.PerBooking, Category = "Praktisk & Komfort", IsActive = true },
                new Service { Id = 16, Name = "Sikker Parkering", Description = "Garanteret parkeringsplads i vores overvågede kælder.", Price = 150.00m, BillingType = BillingType.PerNight, Category = "Praktisk & Komfort", IsActive = true },
                new Service { Id = 17, Name = "Lufthavnstransfer", Description = "Privat luksusbil til eller fra lufthavnen.", Price = 600.00m, BillingType = BillingType.PerBooking, Category = "Praktisk & Komfort", IsActive = true },
                new Service { Id = 18, Name = "Ekstra Rengøring", Description = "Ekstra grundig rengøring af dit værelse under opholdet.", Price = 250.00m, BillingType = BillingType.PerBooking, Category = "Praktisk & Komfort", IsActive = true },
                new Service { Id = 19, Name = "Tøjvask & Strygning", Description = "Få dit tøj vasket, tørret og strøget.", Price = 120.00m, BillingType = BillingType.PerBooking, Category = "Praktisk & Komfort", IsActive = true },
                new Service { Id = 20, Name = "Minibar Refill", Description = "Få minibaren fyldt med dine favorit-drikke og snacks.", Price = 0.00m, BillingType = BillingType.PerBooking, Category = "Praktisk & Komfort", IsActive = true },
                new Service { Id = 21, Name = "Romantisk Pakke", Description = "Rosenblade på sengen, stearinlys og en flaske Cava.", Price = 450.00m, BillingType = BillingType.PerBooking, Category = "Særlige Lejligheder", IsActive = true },
                new Service { Id = 22, Name = "Friske Blomster", Description = "En smuk, frisk buket fra vores lokale florist.", Price = 250.00m, BillingType = BillingType.PerBooking, Category = "Særlige Lejligheder", IsActive = true },
                new Service { Id = 23, Name = "Fødselsdagskage", Description = "En lækker, speciallavet kage til at fejre den store dag.", Price = 300.00m, BillingType = BillingType.PerBooking, Category = "Særlige Lejligheder", IsActive = true },
                new Service { Id = 24, Name = "Butler Service", Description = "En privat butler er tilgængelig 24/7.", Price = 2000.00m, BillingType = BillingType.PerNight, Category = "Særlige Lejligheder", IsActive = true }
            );

            // NYT: Tildeling af hvilke services der er TILGÆNGELIGE for hver værelsestype
            modelBuilder.Entity<RoomType>()
                .HasMany(rt => rt.Services)
                .WithMany(s => s.RoomTypes)
                .UsingEntity<Dictionary<string, object>>(
                    "RoomTypeServices",
                    j => j.HasOne<Service>().WithMany().HasForeignKey("ServicesId"),
                    j => j.HasOne<RoomType>().WithMany().HasForeignKey("RoomTypesId"),
                    j =>
                    {
                        j.HasKey("RoomTypesId", "ServicesId");
                        j.HasData(
                            // Standard Værelse (ID: 1) får 10 services
                            new { RoomTypesId = 1, ServicesId = 1 },   // Morgenmad
                            new { RoomTypesId = 1, ServicesId = 3 },   // Frugtkurv
                            new { RoomTypesId = 1, ServicesId = 4 },   // Chokolade
                            new { RoomTypesId = 1, ServicesId = 8 },   // Snacks
                            new { RoomTypesId = 1, ServicesId = 9 },   // Spa Adgang
                            new { RoomTypesId = 1, ServicesId = 11 },  // Yogamatte
                            new { RoomTypesId = 1, ServicesId = 15 },  // Sen Udtjekning
                            new { RoomTypesId = 1, ServicesId = 16 },  // Parkering
                            new { RoomTypesId = 1, ServicesId = 19 },  // Tøjvask
                            new { RoomTypesId = 1, ServicesId = 20 },  // Minibar Refill

                            // Deluxe Suite (ID: 2) får de 10 fra Standard + 7 ekstra (Total 17)
                            new { RoomTypesId = 2, ServicesId = 1 },   // Morgenmad
                            new { RoomTypesId = 2, ServicesId = 3 },   // Frugtkurv
                            new { RoomTypesId = 2, ServicesId = 4 },   // Chokolade
                            new { RoomTypesId = 2, ServicesId = 8 },   // Snacks
                            new { RoomTypesId = 2, ServicesId = 9 },   // Spa Adgang
                            new { RoomTypesId = 2, ServicesId = 11 },  // Yogamatte
                            new { RoomTypesId = 2, ServicesId = 15 },  // Sen Udtjekning
                            new { RoomTypesId = 2, ServicesId = 16 },  // Parkering
                            new { RoomTypesId = 2, ServicesId = 19 },  // Tøjvask
                            new { RoomTypesId = 2, ServicesId = 20 },  // Minibar Refill
                                                                       // -- Ekstra for Deluxe --
                            new { RoomTypesId = 2, ServicesId = 2 },   // Champagne
                            new { RoomTypesId = 2, ServicesId = 5 },   // Gin & Tonic
                            new { RoomTypesId = 2, ServicesId = 13 },  // Privat Sauna
                            new { RoomTypesId = 2, ServicesId = 14 },  // Badekåbe
                            new { RoomTypesId = 2, ServicesId = 17 },  // Lufthavnstransfer
                            new { RoomTypesId = 2, ServicesId = 18 },  // Ekstra Rengøring
                            new { RoomTypesId = 2, ServicesId = 22 },  // Friske Blomster

                            // Presidential Suite (ID: 3) får alle 24 services
                            new { RoomTypesId = 3, ServicesId = 1 }, new { RoomTypesId = 3, ServicesId = 2 }, new { RoomTypesId = 3, ServicesId = 3 },
                            new { RoomTypesId = 3, ServicesId = 4 }, new { RoomTypesId = 3, ServicesId = 5 }, new { RoomTypesId = 3, ServicesId = 6 },
                            new { RoomTypesId = 3, ServicesId = 7 }, new { RoomTypesId = 3, ServicesId = 8 }, new { RoomTypesId = 3, ServicesId = 9 },
                            new { RoomTypesId = 3, ServicesId = 10 }, new { RoomTypesId = 3, ServicesId = 11 }, new { RoomTypesId = 3, ServicesId = 12 },
                            new { RoomTypesId = 3, ServicesId = 13 }, new { RoomTypesId = 3, ServicesId = 14 }, new { RoomTypesId = 3, ServicesId = 15 },
                            new { RoomTypesId = 3, ServicesId = 16 }, new { RoomTypesId = 3, ServicesId = 17 }, new { RoomTypesId = 3, ServicesId = 18 },
                            new { RoomTypesId = 3, ServicesId = 19 }, new { RoomTypesId = 3, ServicesId = 20 }, new { RoomTypesId = 3, ServicesId = 21 },
                            new { RoomTypesId = 3, ServicesId = 22 }, new { RoomTypesId = 3, ServicesId = 23 }, new { RoomTypesId = 3, ServicesId = 24 }
                        );
                    });
        }
    }
}