using Car_Rent_System.Enums;

namespace Car_Rent_System.DTOs
{
    public class UserDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public string LicenseNumber { get; set; } // For Driver
        public string CompanyName { get; set; }   // For Owner
        public string Address { get; set; }
        public DateTime JoinDate { get; set; }
        public string? ImageUrl { get; set; }
        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
    }
}