using Car_Rent_System.Models;

namespace Car_Rent_System.Services.Interfaces
{
    public interface IDriverService
    {
        Task<Driver?> GetDriverByUserIdAsync(string userId);
        Task<Driver?> GetDriverByIdAsync(int id);
        Task<List<Driver>> GetAllDriversAsync();
        Task<bool> CreateDriverAsync(Driver driver);
        Task<bool> UpdateDriverAsync(Driver driver);
        Task<bool> DeleteDriverAsync(int id);
    }
}