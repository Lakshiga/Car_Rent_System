using System.ComponentModel.DataAnnotations;

namespace Car_Rent_System.Models
{
    public class Owner
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } // FK to ASP.NET Identity User

        [Required, StringLength(100)]
        public string BusinessName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Address { get; set; }

        // Navigation
        public ApplicationUser? User { get; set; }
    }
}