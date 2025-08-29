using Car_Rent_System.Models;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace Car_Rent_System.Models
{
    public class CynexBlazerContext : DbContext
    {
        public CynexBlazerContext(DbContextOptions<CynexBlazerContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        public DbSet<OwnDrivers>OwnDrives { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Customer)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.CustomerID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Car)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CarID)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed data
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserID = 1,
                    Username = "admin",
                    Email = "admin@cynexblazer.com",
                    Password = "admin123",
                    Role = "Admin",
                    FullName = "CYNEX BLAZER Administrator",
                    PhoneNumber = "+1234567890",
                    DateJoined = DateTime.Now
                }
            );

            modelBuilder.Entity<Car>().HasData(
                new Car
                {
                    CarID = 1,
                    CarName = "BMW M5 Competition",
                    CarModel = "2024",
                    ImageUrl = "~/images/BMW.jpg",
                    IsAvailable = true,
                    DailyRate = 299.00m,
                    CarType = "Luxury Sedan",
                    FuelType = "Petrol",
                    SeatingCapacity = 5,
                    Transmission = "Automatic",
                    Description = "Experience ultimate luxury and performance with the BMW M5 Competition.",
                    Mileage = 12.5,
                    DateAdded = DateTime.Now
                },
                new Car
                {
                    CarID = 2,
                    CarName = "Mercedes-AMG G63",
                    CarModel = "2024",
                    ImageUrl = "~/images/Mercedes.jpg",
                    IsAvailable = true,
                    DailyRate = 399.00m,
                    CarType = "Luxury SUV",
                    FuelType = "Petrol",
                    SeatingCapacity = 7,
                    Transmission = "Automatic",
                    Description = "Conquer any terrain with the iconic Mercedes-AMG G63 SUV.",
                    Mileage = 10.2,
                    DateAdded = DateTime.Now
                },
                new Car
                {
                    CarID = 3,
                    CarName = "Lamborghini Huracán",
                    CarModel = "2024",
                    ImageUrl = "~/images/Lambogini.jpg",
                    IsAvailable = true,
                    DailyRate = 899.00m,
                    CarType = "Supercar",
                    FuelType = "Petrol",
                    SeatingCapacity = 2,
                    Transmission = "Automatic",
                    Description = "Feel the thrill of Italian engineering with the Lamborghini Huracán.",
                    Mileage = 8.5,
                    DateAdded = DateTime.Now
                }
            );
        }
    }
}
