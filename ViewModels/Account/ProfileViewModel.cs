using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Car_Rent_System.ViewModels.Account
{
    public class ProfileViewModel
    {
        [Required, StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        public string Address { get; set; }

        public string? ImageUrl { get; set; }

        // Role-specific
        public string? LicenseNumber { get; set; }  // Driver
        public string? CompanyName { get; set; }    // Owner


    }
}