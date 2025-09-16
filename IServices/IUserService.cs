using Car_Rent_System.DTOs;
using Car_Rent_System.Models;
using Car_Rent_System.ViewModels.Account;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Car_Rent_System.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetPendingUsersAsync();
        Task<IEnumerable<UserDto>> GetAllApprovedUsersAsync();
        Task<bool> ApproveUserAsync(string userId);
        Task<bool> RejectUserAsync(string userId);
        Task<ApplicationUser> GetUserByIdAsync(string id);
        Task<ApplicationUser> AuthenticateAsync(string email, string password);
        Task<string?> UploadImageAsync(IFormFile imageFile);
        Task<ApplicationUser> AuthenticateByUsernameAsync(string username, string password);
        
        // Registration methods
        Task<(bool Success, string Message, ApplicationUser User)> RegisterUserAsync(RegisterViewModel model);
        Task<bool> IsEmailExistsAsync(string email);
        Task<bool> IsUsernameExistsAsync(string username);
        Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user);
        
        // Login methods
        Task<(bool Success, string Message, ApplicationUser User)> LoginUserAsync(string emailOrUsername, string password, bool rememberMe);
        Task<bool> ConfirmEmailAsync(string userId, string token);
        Task SignOutAsync();
        
        // Profile methods
        Task<ProfileDto> GetUserProfileAsync(string userId);
        Task<(bool Success, string Message)> UpdateUserProfileAsync(string userId, ProfileDto model, IFormFile? imageFile);
    }
}