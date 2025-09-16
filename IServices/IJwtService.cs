using Car_Rent_System.Models;

namespace Car_Rent_System.Services
{
    public interface IJwtService
    {
        public string GenerateJwtToken(ApplicationUser user);
        object GenJwtToken(ApplicationUser user);
    }
}