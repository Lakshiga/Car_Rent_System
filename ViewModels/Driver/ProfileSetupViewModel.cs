using Microsoft.AspNetCore.Http;

namespace Car_Rent_System.ViewModels.Owner
{
    public class ProfileSetupViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;

        public IFormFile? ImageFile { get; set; }
    }
}