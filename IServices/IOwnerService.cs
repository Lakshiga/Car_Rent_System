using Car_Rent_System.Models;

namespace Car_Rent_System.Services.Interfaces
{
    public interface IOwnerService
    {
        Task<Owner?> GetOwnerByUserIdAsync(string userId);
        Task<Owner?> GetOwnerByIdAsync(int id);
        Task<List<Owner>> GetAllOwnersAsync();
        Task<bool> CreateOwnerAsync(Owner owner);
        Task<bool> UpdateOwnerAsync(Owner owner);
        Task<bool> DeleteOwnerAsync(int id);
    }
}