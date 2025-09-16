using System.ComponentModel.DataAnnotations;

namespace Car_Rent_System.Models
{
    public class Driver
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } // FK to ASP.NET Identity User

        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string LicenseNumber { get; set; } = string.Empty;

        [Required, Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation
        public ApplicationUser? User { get; set; }
    }
}