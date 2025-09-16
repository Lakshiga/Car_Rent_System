using Car_Rent_System.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Car_Rent_System.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // ✅ All DbSets — matching your models
        public DbSet<Car> Cars { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Owner> Owners { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<DamageReport> DamageReports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ Seed Roles
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "0", Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = "1", Name = "SubAdmin", NormalizedName = "SUBADMIN" },
                new IdentityRole { Id = "2", Name = "Staff", NormalizedName = "STAFF" },
                new IdentityRole { Id = "3", Name = "Customer", NormalizedName = "CUSTOMER" },
                new IdentityRole { Id = "4", Name = "Driver", NormalizedName = "DRIVER" },
                new IdentityRole { Id = "5", Name = "VehicleOwner", NormalizedName = "VEHICLEOWNER" }
            );

            // ✅ Seed Payment Methods
            modelBuilder.Entity<PaymentMethod>().HasData(
                new PaymentMethod { Id = 1, Name = "Cash", Description = "Cash payment", IsActive = true, CreatedAt = DateTime.UtcNow },
                new PaymentMethod { Id = 2, Name = "Credit Card", Description = "Credit card payment", IsActive = true, CreatedAt = DateTime.UtcNow },
                new PaymentMethod { Id = 3, Name = "Bank Transfer", Description = "Bank transfer payment", IsActive = true, CreatedAt = DateTime.UtcNow },
                new PaymentMethod { Id = 4, Name = "Stripe", Description = "Stripe payment gateway", IsActive = true, CreatedAt = DateTime.UtcNow }
            );

            // ⚙️ Configure Car
            modelBuilder.Entity<Car>(entity =>
            {
                entity.HasKey(c => c.Id);

                // ✅ Car → Owner (ApplicationUser)
                entity.HasOne(c => c.Owner) // 👈 Single reference
                      .WithMany(u => u.Cars) // 👈 Collection on ApplicationUser
                      .HasForeignKey(c => c.ApplicationUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ⚙️ Configure Booking
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(b => b.BookingID);

                // ✅ Booking → Customer
                entity.HasOne(b => b.Customer)
                      .WithMany(u => u.Bookings)
                      .HasForeignKey(b => b.CustomerID)
                      .OnDelete(DeleteBehavior.Restrict);

                // ✅ Booking → Car
                entity.HasOne(b => b.Car)
                      .WithMany(c => c.Bookings)
                      .HasForeignKey(b => b.CarID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ⚙️ Configure Driver
            modelBuilder.Entity<Driver>(entity =>
            {
                entity.HasKey(d => d.Id);

                // Relationship: Driver → ApplicationUser
                entity.HasOne(d => d.User)
                      .WithMany()
                      .HasForeignKey(d => d.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ⚙️ Configure Owner
            modelBuilder.Entity<Owner>(entity =>
            {
                entity.HasKey(o => o.Id);

                // Relationship: Owner → ApplicationUser
                entity.HasOne(o => o.User)
                      .WithMany()
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ⚙️ Configure PaymentMethod
            modelBuilder.Entity<PaymentMethod>(entity =>
            {
                entity.HasKey(pm => pm.Id);
                entity.HasIndex(pm => pm.Name).IsUnique();
            });

            // ⚙️ Configure Payment
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(p => p.Id);

                // Payment → PaymentMethod
                entity.HasOne(p => p.PaymentMethod)
                      .WithMany(pm => pm.Payments)
                      .HasForeignKey(p => p.PaymentMethodId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Payment → Booking
                entity.HasOne(p => p.Booking)
                      .WithMany()
                      .HasForeignKey(p => p.BookingId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ⚙️ Configure DamageReport
            modelBuilder.Entity<DamageReport>(entity =>
            {
                entity.HasKey(d => d.ReportID);

                // Relationship: DamageReport → Booking
                entity.HasOne(d => d.Booking)
                      .WithMany()
                      .HasForeignKey(d => d.BookingID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

                       

            // ❌ REMOVED: .HasData() for Cars — will be seeded at runtime in Program.cs
        }
    }
}