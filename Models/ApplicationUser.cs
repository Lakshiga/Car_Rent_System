using System.ComponentModel.DataAnnotations.Schema;
using Car_Rent_System.Enums;
using Microsoft.AspNetCore.Identity;

namespace Car_Rent_System.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string? Address { get; set; }
        public string? LicenseNumber { get; set; }
        public string? CompanyName { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public Role Role { get; set; } = Role.Customer;

        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;

        public DateTime JoinDate { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Car> Cars { get; set; } = new List<Car>();
    }
}