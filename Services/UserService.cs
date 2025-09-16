using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Car_Rent_System.DTOs;
using Car_Rent_System.Enums;
using Car_Rent_System.Data;
using Car_Rent_System.Interfaces;
using Car_Rent_System.Models;
using Car_Rent_System.ViewModels.Account;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Car_Rent_System.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly Cloudinary _cloudinary;

        public UserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context, Cloudinary cloudinary = null)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _context = context;
            _cloudinary = cloudinary;
        }

        public async Task<IEnumerable<UserDto>> GetPendingUsersAsync()
        {
            var users = await _context.Users
                .Where(u => u.VerificationStatus == VerificationStatus.Pending)
                .OrderBy(u => u.JoinDate)
                .ToListAsync();

            return users.Select(MapToDto);
        }

        public async Task<IEnumerable<UserDto>> GetAllApprovedUsersAsync()
        {
            var users = await _context.Users
                .Where(u => u.VerificationStatus == VerificationStatus.Approved)
                .OrderBy(u => u.UserName)
                .ToListAsync();

            return users.Select(MapToDto);
        }

        public async Task<bool> ApproveUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.VerificationStatus = VerificationStatus.Approved;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> RejectUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.VerificationStatus = VerificationStatus.Rejected;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0 || _cloudinary == null) return string.Empty;

            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult?.SecureUrl?.ToString() ?? string.Empty;
        }

        private UserDto MapToDto(ApplicationUser user)
        {
            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString(),
                LicenseNumber = user.LicenseNumber,
                CompanyName = user.CompanyName,
                Address = user.Address,
                ImageUrl = user.ImageUrl,
                VerificationStatus = user.VerificationStatus,
                JoinDate = user.JoinDate // Fixed: Removed the null-coalescing operator
            };
        }

        public async Task<ApplicationUser> AuthenticateAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null && await _userManager.CheckPasswordAsync(user, password))
            {
                return user;
            }
            return null;
        }

        public async Task<ApplicationUser> AuthenticateByUsernameAsync(string username, string password)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user != null && await _userManager.CheckPasswordAsync(user, password))
            {
                return user;
            }
            return null;
        }

        // Registration methods
        public async Task<(bool Success, string Message, ApplicationUser User)> RegisterUserAsync(RegisterViewModel model)
        {
            try
            {
                // Validate role
                var allowedRoles = new[] { Car_Rent_System.Enums.Role.Customer.ToString(), Car_Rent_System.Enums.Role.Driver.ToString(), Car_Rent_System.Enums.Role.VehicleOwner.ToString() };
                if (!allowedRoles.Contains(model.RequestedRole))
                {
                    return (false, "Invalid role selection.", null);
                }

                // Check if email already exists
                if (await IsEmailExistsAsync(model.Email))
                {
                    return (false, "Email already registered.", null);
                }

                // Check if username already exists
                if (await IsUsernameExistsAsync(model.Username))
                {
                    return (false, "Username already taken.", null);
                }

                // Create user with correct verification status
                var user = new ApplicationUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    FirstName = model.FullName.Split(' ').FirstOrDefault() ?? "",
                    LastName = model.FullName.Split(' ').LastOrDefault() ?? "",
                    PhoneNumber = model.PhoneNumber,
                    Role = Enum.Parse<Car_Rent_System.Enums.Role>(model.RequestedRole),
                    // Auto-approve customers — pending for driver/owner
                    VerificationStatus = model.RequestedRole == Car_Rent_System.Enums.Role.Customer.ToString()
                        ? VerificationStatus.Approved
                        : VerificationStatus.Pending,
                    JoinDate = DateTime.UtcNow,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Ensure role exists
                    var roleExists = await _roleManager.RoleExistsAsync(model.RequestedRole);
                    if (!roleExists)
                    {
                        var roleResult = await _roleManager.CreateAsync(new IdentityRole(model.RequestedRole));
                        if (!roleResult.Succeeded)
                        {
                            return (false, "Failed to create role.", null);
                        }
                    }

                    await _userManager.AddToRoleAsync(user, model.RequestedRole);

                    var message = user.VerificationStatus == VerificationStatus.Approved
                        ? "Registration successful! Welcome to CYNEX BLAZER!"
                        : "Registration successful! Please wait for admin approval.";

                    return (true, message, user);
                }

                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, errors, null);
            }
            catch (Exception ex)
            {
                return (false, $"Registration failed: {ex.Message}", null);
            }
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email) != null;
        }

        public async Task<bool> IsUsernameExistsAsync(string username)
        {
            return await _userManager.FindByNameAsync(username) != null;
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        // Login methods
        public async Task<(bool Success, string Message, ApplicationUser User)> LoginUserAsync(string emailOrUsername, string password, bool rememberMe)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(emailOrUsername)
                          ?? await _userManager.FindByNameAsync(emailOrUsername);

                if (user == null || !await _userManager.CheckPasswordAsync(user, password))
                {
                    return (false, "Invalid login attempt.", null);
                }

                // Check verification status
                if (user.VerificationStatus == null || user.VerificationStatus != VerificationStatus.Approved)
                {
                    return (false, "Your account is pending admin approval.", null);
                }

                // Sync role from database
                var userRoles = await _userManager.GetRolesAsync(user);
                var userRole = userRoles.FirstOrDefault() ?? "Customer";

                if (user.Role == null || user.Role.ToString() != userRole)
                {
                    if (Enum.TryParse<Car_Rent_System.Enums.Role>(userRole, out var parsedRole))
                    {
                        user.Role = parsedRole;
                        await _userManager.UpdateAsync(user);
                    }
                }

                return (true, "Login successful", user);
            }
            catch (Exception ex)
            {
                return (false, $"Login failed: {ex.Message}", null);
            }
        }

        public async Task<bool> ConfirmEmailAsync(string userId, string token)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return false;

                var result = await _userManager.ConfirmEmailAsync(user, token);
                return result.Succeeded;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        // Profile methods
        public async Task<ProfileDto> GetUserProfileAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return null;

                return new ProfileDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    ImageUrl = user.ImageUrl,
                    LicenseNumber = user.LicenseNumber,
                    CompanyName = user.CompanyName
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<(bool Success, string Message)> UpdateUserProfileAsync(string userId, ProfileDto model, IFormFile? imageFile)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return (false, "User not found.");
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.Address = model.Address;

                if (user.Role == Car_Rent_System.Enums.Role.Driver)
                {
                    user.LicenseNumber = model.LicenseNumber;
                }
                else if (user.Role == Car_Rent_System.Enums.Role.VehicleOwner)
                {
                    user.CompanyName = model.CompanyName;
                }

                if (imageFile != null && imageFile.Length > 0)
                {
                    var imageUrl = await UploadImageAsync(imageFile);
                    user.ImageUrl = imageUrl;
                }

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return (true, "Profile updated successfully!");
                }

                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, errors);
            }
            catch (Exception ex)
            {
                return (false, $"Profile update failed: {ex.Message}");
            }
        }
    }
}