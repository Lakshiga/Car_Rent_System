using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Car_Rent_System.Models;


namespace Car_Rent_System.Models
{
    public class Booking
    {
        [Key]
        public int BookingID { get; set; }

        [Required]
        [ForeignKey("Customer")]
        public int CustomerID { get; set; }

        [Required]
        [ForeignKey("Car")]
        public int CarID { get; set; }

        [Required(ErrorMessage = "Pickup date is required")]
        [Display(Name = "Pickup Date")]
        [DataType(DataType.Date)]
        public DateTime PickupDate { get; set; }

        [Required(ErrorMessage = "Return date is required")]
        [Display(Name = "Return Date")]
        [DataType(DataType.Date)]
        public DateTime ReturnDate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Total Cost")]
        public decimal TotalCost { get; set; }

        [StringLength(20)]
        [Display(Name = "Booking Status")]
        public string BookingStatus { get; set; } = "Confirmed";

        [Display(Name = "Booking Date")]
        public DateTime BookingDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        [Display(Name = "Special Requirements")]
        public string SpecialRequirements { get; set; }

        [Display(Name = "Pickup Location")]
        [StringLength(200)]
        public string PickupLocation { get; set; }

        [Display(Name = "Return Location")]
        [StringLength(200)]
        public string ReturnLocation { get; set; }

        // Navigation properties
        public virtual User Customer { get; set; }
        public virtual Car Car { get; set; }

        // Calculated property
        [NotMapped]
        public int TotalDays => (ReturnDate - PickupDate).Days + 1;
    }
}
