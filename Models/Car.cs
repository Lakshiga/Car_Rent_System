using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Car_Rent_System.Models
{
    public class Car
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Car name is required")]
        [StringLength(100)]
        [Display(Name = "Car Name")]
        public string CarName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Model Year")]
        public string CarModel { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Available")]
        public bool IsAvailable { get; set; } = true;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Daily Rate ($)")]
        [Range(0.01, 9999.99)]
        public decimal DailyRate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Per Kilometer Rate ($)")]
        [Range(0.01, 99.99)]
        public decimal? PerKilometerRate { get; set; }

        [StringLength(50)]
        [Display(Name = "Car Type")]
        public string? CarType { get; set; }

        [StringLength(20)]
        [Display(Name = "Fuel Type")]
        public string? FuelType { get; set; }

        [Display(Name = "Seating Capacity")]
        [Range(2, 12)]
        public int? SeatingCapacity { get; set; }

        [StringLength(20)]
        [Display(Name = "Transmission")]
        public string? Transmission { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Mileage (km/l)")]
        public double? Mileage { get; set; }

        [Display(Name = "Date Added")]
        public DateTime DateAdded { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string? Make { get; set; }

        public int? Year { get; set; }

        [Required(ErrorMessage = "Number plate is required")]
        [StringLength(20, ErrorMessage = "Number plate cannot exceed 20 characters")]
        [Display(Name = "Number Plate")]
        public string NumberPlate { get; set; } = string.Empty;

        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser? Owner { get; set; }

        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        
        [StringLength(450)]
        public string? ApplicationUserId { get; set; }
    }
}