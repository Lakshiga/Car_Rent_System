using System.ComponentModel.DataAnnotations;

namespace Car_Rent_System.ViewModels.Car
{
    public class CarViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Make { get; set; } = string.Empty;

        public required string CarName { get; set; }

        [Required]
        public string Model { get; set; } = string.Empty;

        // Add CarModel property to match Car model
        public string CarModel { get; set; } = string.Empty;

        [Required]
        public int Year { get; set; }

        [Required]
        public string Color { get; set; } = string.Empty;

        [Required]
        public string LicensePlate { get; set; } = string.Empty;

        [Required]
        public decimal DailyRate { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsAvailable { get; set; }

        public string Status { get; set; } = "Available";

        public IFormFile? ImageFile { get; set; }

        // Add properties to match Car model
        public string? CarType { get; set; }
        public string? FuelType { get; set; }
        public string? Transmission { get; set; }
        public int? SeatingCapacity { get; set; }
        public double? Mileage { get; set; }
        public string? Description { get; set; }

        public string DisplayName => $"{Year} {Make} {Model}";
    }
}