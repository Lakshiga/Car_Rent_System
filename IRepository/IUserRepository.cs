using Car_Rent_System.DTOs;
using Car_Rent_System.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Car_Rent_System.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto> GetUserDtoByIdAsync(string id);
        Task<UserDto> GetUserByEmailAsync(string email);
        Task<UserDto> CreateUserAsync(UserDto userDto);
        Task UpdateUserDtoAsync(UserDto userDto);
        Task DeleteUserAsync(string userId);
        Task<bool> ApproveUserAsync(string userId);
        Task<bool> RejectUserAsync(string userId);
        
        // Registration methods
        Task<ApplicationUser> CreateUserWithPasswordAsync(ApplicationUser user, string password);
        Task<bool> AddUserToRoleAsync(ApplicationUser user, string role);
        Task<bool> CheckEmailExistsAsync(string email);
        Task<bool> CheckUsernameExistsAsync(string username);
        Task<bool> CreateRoleIfNotExistsAsync(string role);
        
        // Login methods
        Task<ApplicationUser> FindUserByEmailOrUsernameAsync(string emailOrUsername);
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
        Task<bool> ConfirmEmailAsync(ApplicationUser user, string token);
        Task<IList<string>> GetUserRolesAsync(ApplicationUser user);
        Task UpdateUserAsync(ApplicationUser user);
        
        // Profile methods
        Task<ApplicationUser> GetUserByIdAsync(string userId);
        Task<bool> UpdateUserProfileAsync(ApplicationUser user);
    }
}