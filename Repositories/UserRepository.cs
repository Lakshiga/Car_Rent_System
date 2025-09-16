using Car_Rent_System.DTOs;
using Car_Rent_System.Enums;
using Car_Rent_System.Data;
using Car_Rent_System.Interfaces;
using Car_Rent_System.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Car_Rent_System.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users.ToListAsync();
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = roles.FirstOrDefault() ?? "Customer",
                    LicenseNumber = user.LicenseNumber,
                    CompanyName = user.CompanyName,
                    Address = user.Address,
                    ImageUrl = user.ImageUrl,
                    VerificationStatus = user.VerificationStatus
                });
            }

            return userDtos;
        }

        public async Task<UserDto> GetUserDtoByIdAsync(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = roles.FirstOrDefault() ?? "Customer",
                LicenseNumber = user.LicenseNumber,
                CompanyName = user.CompanyName,
                Address = user.Address,
                ImageUrl = user.ImageUrl,
                VerificationStatus = user.VerificationStatus
            };
        }

        public async Task<UserDto> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = roles.FirstOrDefault() ?? "Customer",
                LicenseNumber = user.LicenseNumber,
                CompanyName = user.CompanyName,
                Address = user.Address,
                ImageUrl = user.ImageUrl,
                VerificationStatus = user.VerificationStatus
            };
        }

        public async Task<UserDto> CreateUserAsync(UserDto userDto)
        {
            var user = new ApplicationUser
            {
                UserName = userDto.Email,
                Email = userDto.Email,
                FirstName = userDto.FullName.Split(' ').FirstOrDefault(),
                LastName = userDto.FullName.Split(' ').LastOrDefault(),
                PhoneNumber = userDto.PhoneNumber,
                Address = userDto.Address,
                LicenseNumber = userDto.LicenseNumber,
                CompanyName = userDto.CompanyName,
                VerificationStatus = userDto.VerificationStatus,
                JoinDate = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, "Default@123"); // Set default password or use a secure one

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, userDto.Role ?? "Customer");
            }

            return userDto;
        }

        public async Task UpdateUserDtoAsync(UserDto userDto)
        {
            var user = await _context.Users.FindAsync(userDto.Id);
            if (user != null)
            {
                user.FirstName = userDto.FullName.Split(' ').FirstOrDefault();
                user.LastName = userDto.FullName.Split(' ').LastOrDefault();
                user.PhoneNumber = userDto.PhoneNumber;
                user.Address = userDto.Address;
                user.LicenseNumber = userDto.LicenseNumber;
                user.CompanyName = userDto.CompanyName;
                user.ImageUrl = userDto.ImageUrl;
                user.VerificationStatus = userDto.VerificationStatus;

                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteUserAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ApproveUserAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.VerificationStatus = VerificationStatus.Approved;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> RejectUserAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.VerificationStatus = VerificationStatus.Rejected;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        // Registration methods
        public async Task<ApplicationUser> CreateUserWithPasswordAsync(ApplicationUser user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            return result.Succeeded ? user : null;
        }

        public async Task<bool> AddUserToRoleAsync(ApplicationUser user, string role)
        {
            var result = await _userManager.AddToRoleAsync(user, role);
            return result.Succeeded;
        }

        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user != null;
        }

        public async Task<bool> CheckUsernameExistsAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            return user != null;
        }

        public async Task<bool> CreateRoleIfNotExistsAsync(string role)
        {
            var roleExists = await _roleManager.RoleExistsAsync(role);
            if (!roleExists)
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(role));
                return result.Succeeded;
            }
            return true;
        }

        // Login methods
        public async Task<ApplicationUser> FindUserByEmailOrUsernameAsync(string emailOrUsername)
        {
            return await _userManager.FindByEmailAsync(emailOrUsername)
                   ?? await _userManager.FindByNameAsync(emailOrUsername);
        }

        public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<bool> ConfirmEmailAsync(ApplicationUser user, string token)
        {
            var result = await _userManager.ConfirmEmailAsync(user, token);
            return result.Succeeded;
        }

        public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task UpdateUserAsync(ApplicationUser user)
        {
            await _userManager.UpdateAsync(user);
        }

        // Profile methods
        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<bool> UpdateUserProfileAsync(ApplicationUser user)
        {
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
    }
}
