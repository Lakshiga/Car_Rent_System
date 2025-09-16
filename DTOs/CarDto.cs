using System.ComponentModel.DataAnnotations;

namespace Car_Rent_System.DTOs
{
    public class CarDto
    {
        public int CarID { get; set; }

        [Required(ErrorMessage = "Car name is required")]
        [StringLength(100, ErrorMessage = "Car name cannot exceed 100 characters")]
        public string CarName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Model year is required")]
        public string CarModel { get; set; } = string.Empty;

        [Required(ErrorMessage = "Car type is required")]
        public string CarType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Daily rate is required")]
        [Range(0.01, 10000, ErrorMessage = "Daily rate must be between $0.01 and $10,000")]
        public decimal DailyRate { get; set; }

        [Range(0.01, 100, ErrorMessage = "Per kilometer rate must be between $0.01 and $100")]
        public decimal? PerKilometerRate { get; set; }

        [Required(ErrorMessage = "Fuel type is required")]
        public string FuelType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Transmission is required")]
        public string Transmission { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seating capacity is required")]
        public int? SeatingCapacity { get; set; }

        public decimal? Mileage { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsAvailable { get; set; } = true;
        public DateTime? DateAdded { get; set; }
        public int Id { get; internal set; }
    }
}