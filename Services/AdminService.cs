using Car_Rent_System.DTOs;
using Car_Rent_System.Enums;
using Car_Rent_System.Interfaces;
using Car_Rent_System.Models;
using Car_Rent_System.ViewModels.Admin;
using Car_Rent_System.ViewModels.Booking;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Identity;

namespace Car_Rent_System.Services
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepository;
        private readonly Cloudinary _cloudinary;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminService(IAdminRepository adminRepository, Cloudinary cloudinary, UserManager<ApplicationUser> userManager)
        {
            _adminRepository = adminRepository;
            _cloudinary = cloudinary;
            _userManager = userManager;
        }

        // Dashboard methods
        public async Task<string> GetDashboardTitleAsync(string userId)
        {
            try
            {
                var user = await _adminRepository.GetUserByIdAsync(userId);
                if (user == null) return "Dashboard";

                var userRoles = await _adminRepository.GetUserRolesAsync(user);
                var userRole = userRoles.FirstOrDefault();

                if (userRole == Car_Rent_System.Enums.Role.Admin.ToString())
                    return "ADMIN DASHBOARD";
                else if (userRole == Car_Rent_System.Enums.Role.SubAdmin.ToString())
                    return "SUB-ADMIN DASHBOARD";
                else if (userRole == Car_Rent_System.Enums.Role.Staff.ToString())
                    return "STAFF DASHBOARD";
                else
                    return "DASHBOARD";
            }
            catch (Exception)
            {
                return "Dashboard";
            }
        }

        public async Task<(int TotalCars, int AvailableCars, int TotalBookings, decimal TotalRevenue)> GetDashboardStatsAsync()
        {
            try
            {
                return await _adminRepository.GetDashboardStatsAsync();
            }
            catch (Exception)
            {
                return (0, 0, 0, 0);
            }
        }

        public async Task<IEnumerable<BookingViewModel>> GetRecentBookingsAsync(int count)
        {
            try
            {
                return await _adminRepository.GetRecentBookingsAsync(count);
            }
            catch (Exception)
            {
                return new List<BookingViewModel>();
            }
        }

        // User management methods
        public async Task<IEnumerable<UserDto>> GetPendingUsersAsync()
        {
            try
            {
                return await _adminRepository.GetPendingUsersAsync();
            }
            catch (Exception)
            {
                return new List<UserDto>();
            }
        }

        public async Task<IEnumerable<UserDto>> GetAllApprovedUsersAsync()
        {
            try
            {
                return await _adminRepository.GetAllApprovedUsersAsync();
            }
            catch (Exception)
            {
                return new List<UserDto>();
            }
        }

        public async Task<(bool Success, string Message)> ApproveUserAsync(string userId)
        {
            try
            {
                var success = await _adminRepository.ApproveUserAsync(userId);
                return success 
                    ? (true, "User approved successfully!") 
                    : (false, "Failed to approve user.");
            }
            catch (Exception ex)
            {
                return (false, $"Error approving user: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> RejectUserAsync(string userId)
        {
            try
            {
                var success = await _adminRepository.RejectUserAsync(userId);
                return success 
                    ? (true, "User rejected successfully!") 
                    : (false, "Failed to reject user.");
            }
            catch (Exception ex)
            {
                return (false, $"Error rejecting user: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateUserRoleAsync(string userId, string newRole)
        {
            try
            {
                var user = await _adminRepository.GetUserByIdAsync(userId);
                if (user == null)
                    return (false, "User not found");

                var success = await _adminRepository.UpdateUserRoleAsync(user, newRole);
                return success 
                    ? (true, "User role updated successfully") 
                    : (false, "Failed to update user role");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating user role: {ex.Message}");
            }
        }

        // Car management methods
        public async Task<IEnumerable<CarDto>> GetAllCarsAsync()
        {
            try
            {
                return await _adminRepository.GetAllCarsAsync();
            }
            catch (Exception)
            {
                return new List<CarDto>();
            }
        }

        public async Task<(bool Success, string Message)> CreateCarAsync(CarDto carDto, IFormFile? imageFile, string adminUserId)
        {
            try
            {
                string imageUrl = string.Empty;

                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    try
                    {
                        var uploadParams = new ImageUploadParams
                        {
                            File = new FileDescription(imageFile.FileName, imageFile.OpenReadStream()),
                            UseFilename = true,
                            UniqueFilename = true,
                            Overwrite = false
                        };

                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                        imageUrl = uploadResult?.SecureUrl?.ToString() ?? string.Empty;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Cloudinary upload error: {ex.Message}");
                        // Continue without image if Cloudinary fails
                        imageUrl = string.Empty;
                    }
                }

                // Use a default image if no image was uploaded
                if (string.IsNullOrEmpty(imageUrl))
                {
                    imageUrl = "/images/default-car.jpg"; // Default image path
                }

                var success = await _adminRepository.CreateCarAsync(carDto, imageUrl, adminUserId);
                return success 
                    ? (true, "Car added successfully to the fleet!") 
                    : (false, "Failed to add car to the fleet.");
            }
            catch (Exception ex)
            {
                return (false, $"Error adding car: {ex.Message}");
            }
        }

        public async Task<CarDto?> GetCarByIdAsync(int carId)
        {
            try
            {
                return await _adminRepository.GetCarByIdAsync(carId);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<(bool Success, string Message)> UpdateCarAsync(CarDto carDto)
        {
            try
            {
                var success = await _adminRepository.UpdateCarAsync(carDto);
                return success 
                    ? (true, "Car details updated successfully!") 
                    : (false, "Failed to update car details.");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating car: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteCarAsync(int carId)
        {
            try
            {
                var success = await _adminRepository.DeleteCarAsync(carId);
                return success 
                    ? (true, "Car removed from fleet!") 
                    : (false, "Cannot delete car with active bookings.");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting car: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ToggleCarAvailabilityAsync(int carId)
        {
            try
            {
                var success = await _adminRepository.ToggleCarAvailabilityAsync(carId);
                return success 
                    ? (true, "Availability updated") 
                    : (false, "Failed to update availability");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating availability: {ex.Message}");
            }
        }

        // Booking management methods
        public async Task<IEnumerable<BookingViewModel>> GetAllBookingsAsync()
        {
            try
            {
                return await _adminRepository.GetAllBookingsAsync();
            }
            catch (Exception)
            {
                return new List<BookingViewModel>();
            }
        }

        public async Task<bool> UpdateBookingStatusAsync(int bookingId, string status)
        {
            try
            {
                return await _adminRepository.UpdateBookingStatusAsync(bookingId, status);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ UpdateBookingStatusAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ProcessRentAsync(int bookingId, int odometerStartReading, string paymentMethod, decimal partialPayment)
        {
            try
            {
                return await _adminRepository.ProcessRentAsync(bookingId, odometerStartReading, paymentMethod, partialPayment);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ProcessRentAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ProcessReturnAsync(int bookingId, DateTime returnDate, int odometerEndReading, string paymentStatus)
        {
            try
            {
                return await _adminRepository.ProcessReturnAsync(bookingId, returnDate, odometerEndReading, paymentStatus);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ProcessReturnAsync error: {ex.Message}");
                return false;
            }
        }

        // Admin password reset
        public async Task<(bool Success, string Message)> ResetAdminPasswordAsync(string adminEmail, string newPassword)
        {
            try
            {
                var success = await _adminRepository.ResetAdminPasswordAsync(adminEmail, newPassword);
                return success 
                    ? (true, $"Admin password reset to: {newPassword}") 
                    : (false, "Failed to reset admin password.");
            }
            catch (Exception ex)
            {
                return (false, $"Error resetting password: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> CreateUserAsync(CreateUserViewModel model)
        {
            try
            {
                var user = new ApplicationUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    FirstName = model.FullName.Split(' ')[0],
                    LastName = model.FullName.Split(' ').Length > 1 ? string.Join(" ", model.FullName.Split(' ').Skip(1)) : "",
                    PhoneNumber = model.PhoneNumber,
                    Role = Enum.Parse<Car_Rent_System.Enums.Role>(model.SelectedRole),
                    VerificationStatus = VerificationStatus.Approved,
                    JoinDate = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    return (true, "User created successfully.");
                }
                else
                {
                    return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error creating user: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateUserAsync(EditUserViewModel model)
        {
            try
            {
                var user = await _adminRepository.GetUserByIdAsync(model.Id);
                if (user == null)
                    return (false, "User not found.");

                user.FirstName = model.FullName.Split(' ')[0];
                user.LastName = model.FullName.Split(' ').Length > 1 ? string.Join(" ", model.FullName.Split(' ').Skip(1)) : "";
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.Role = Enum.Parse<Car_Rent_System.Enums.Role>(model.SelectedRole);

                await _adminRepository.UpdateUserAsync(user);
                return (true, "User updated successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating user: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteUserAsync(string userId)
        {
            try
            {
                var user = await _adminRepository.GetUserByIdAsync(userId);
                if (user == null)
                    return (false, "User not found.");

                if (user.Role == Car_Rent_System.Enums.Role.Admin)
                    return (false, "Cannot delete admin user.");

                await _adminRepository.DeleteUserAsync(userId);
                return (true, "User deleted successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting user: {ex.Message}");
            }
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            try
            {
                return await _adminRepository.GetAllUsersAsync();
            }
            catch (Exception)
            {
                return new List<ApplicationUser>();
            }
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            try
            {
                return await _adminRepository.GetUserByIdAsync(userId);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> ClearAllCarsAsync()
        {
            try
            {
                return await _adminRepository.ClearAllCarsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ClearAllCarsAsync error: {ex.Message}");
                return false;
            }
        }
    }
}
