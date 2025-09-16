using Car_Rent_System.DTOs;
using Car_Rent_System.Models;
using Car_Rent_System.ViewModels.Admin;
using Car_Rent_System.ViewModels.Booking;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Car_Rent_System.Interfaces
{
    public interface IAdminService
    {
        // Dashboard methods
        Task<string> GetDashboardTitleAsync(string userId);
        Task<(int TotalCars, int AvailableCars, int TotalBookings, decimal TotalRevenue)> GetDashboardStatsAsync();
        Task<IEnumerable<BookingViewModel>> GetRecentBookingsAsync(int count);
        
        // User management methods
        Task<IEnumerable<UserDto>> GetPendingUsersAsync();
        Task<IEnumerable<UserDto>> GetAllApprovedUsersAsync();
        Task<(bool Success, string Message)> ApproveUserAsync(string userId);
        Task<(bool Success, string Message)> RejectUserAsync(string userId);
        Task<(bool Success, string Message)> UpdateUserRoleAsync(string userId, string newRole);
        
        // Car management methods
        Task<IEnumerable<CarDto>> GetAllCarsAsync();
        Task<(bool Success, string Message)> CreateCarAsync(CarDto carDto, IFormFile? imageFile, string adminUserId);
        Task<CarDto?> GetCarByIdAsync(int carId);
        Task<(bool Success, string Message)> UpdateCarAsync(CarDto carDto);
        Task<(bool Success, string Message)> DeleteCarAsync(int carId);
        Task<(bool Success, string Message)> ToggleCarAvailabilityAsync(int carId);
        
        // Booking management methods
        Task<IEnumerable<BookingViewModel>> GetAllBookingsAsync();
        Task<bool> UpdateBookingStatusAsync(int bookingId, string status);
        Task<bool> ProcessRentAsync(int bookingId, int odometerStartReading, string paymentMethod, decimal partialPayment);
        Task<bool> ProcessReturnAsync(int bookingId, DateTime returnDate, int odometerEndReading, string paymentStatus);
        
        // Admin password reset
        Task<(bool Success, string Message)> ResetAdminPasswordAsync(string adminEmail, string newPassword);
        
        // User management methods
        Task<(bool Success, string Message)> CreateUserAsync(CreateUserViewModel model);
        Task<(bool Success, string Message)> UpdateUserAsync(EditUserViewModel model);
        Task<(bool Success, string Message)> DeleteUserAsync(string userId);
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
        Task<ApplicationUser?> GetUserByIdAsync(string userId);
        
        // Database management methods
        Task<bool> ClearAllCarsAsync();
    }
}
