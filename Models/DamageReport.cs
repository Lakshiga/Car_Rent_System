using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Car_Rent_System.Models
{
    public class DamageReport
    {
        [Key]
        public int ReportID { get; set; }

        [Required]
        public int BookingID { get; set; }

        [StringLength(20)]
        public string ReportedBy { get; set; } = "Customer"; // "Customer", "Staff"

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(200)]
        public string? ImageUrl { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal FineAmount { get; set; }

        [StringLength(20)]
        public string ReportStatus { get; set; } = "Pending"; // "Pending", "Reviewed", "Resolved"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("BookingID")]
        public virtual Booking Booking { get; set; } = null!;
    }
}