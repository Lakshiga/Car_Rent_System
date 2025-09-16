using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Car_Rent_System.Enums;

namespace Car_Rent_System.Models
{
    public class Booking
    {
        [Key]
        public int BookingID { get; set; }

        [Required]
        public string CustomerID { get; set; } = string.Empty;

        [Required]
        public int CarID { get; set; }

        [Required]
        public DateTime PickupDate { get; set; }

        [Required]
        public DateTime ReturnDate { get; set; }

        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost { get; set; }

        public BookingStatus BookingStatus { get; set; } = BookingStatus.Pending;

        public string PickupLocation { get; set; } = string.Empty;

        public string? ReturnLocation { get; set; }

        [StringLength(500)]
        public string? SpecialRequirements { get; set; }
        public string? StripeSessionId { get; set; }

        // New booking details for enhanced flow
        [StringLength(50)]
        public string? LicenseNumber { get; set; }

        [StringLength(20)]
        public string? NICNumber { get; set; }

        [StringLength(500)]
        public string? DocumentImageUrl { get; set; }

        [StringLength(500)]
        public string? LicenseImageFrontUrl { get; set; }

        [StringLength(500)]
        public string? LicenseImageBackUrl { get; set; }

        public int? OdometerStartReading { get; set; }

        public int? OdometerEndReading { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DistanceTraveled { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PerKilometerRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? AdvancePayment { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? RemainingAmount { get; set; }

        // Navigation properties
        [ForeignKey("CustomerID")]
        public virtual ApplicationUser Customer { get; set; } = null!;

        [ForeignKey("CarID")]
        public virtual Car Car { get; set; } = null!;
        public bool WithDriver { get; set; } = false;
    }
}