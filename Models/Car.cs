using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Car_Rent_System.Models
{
    public class Car
    {
        [Key]


        // models update
        public int CarID { get; set; }

        [Required(ErrorMessage = "Car name is required")]
        [StringLength(100, ErrorMessage = "Car name cannot exceed 100 characters")]
        [Display(Name = "Car Name")]
        public string CarName { get; set; }

        [Required(ErrorMessage = "Car model is required")]
        [StringLength(50, ErrorMessage = "Car model cannot exceed 50 characters")]
        [Display(Name = "Model Year")]
        public string CarModel { get; set; }`

        [StringLength(200)]
        [Display(Name = "Image URL")]
        public string ImageUrl { get; set; }

        [Display(Name = "Available")]
        public bool IsAvailable { get; set; } = true;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Daily Rate ($)")]
        [Range(0.01, 9999.99, ErrorMessage = "Daily rate must be between $0.01 and $9999.99")]
        public decimal DailyRate { get; set; }

        [StringLength(50)]
        [Display(Name = "Car Type")]
        public string CarType { get; set; }

        [StringLength(20)]
        [Display(Name = "Fuel Type")]
        public string FuelType { get; set; }

        [Display(Name = "Seating Capacity")]
        [Range(2, 12, ErrorMessage = "Seating capacity must be between 2 and 12")]
        public int SeatingCapacity { get; set; }

        [StringLength(20)]
        [Display(Name = "Transmission")]
        public string Transmission { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Display(Name = "Mileage (km/l)")]
        public double? Mileage { get; set; }

        [Display(Name = "Date Added")]
        public DateTime DateAdded { get; set; } = DateTime.Now;

        // Navigation property
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
