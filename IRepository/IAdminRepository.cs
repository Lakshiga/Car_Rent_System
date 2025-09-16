using Car_Rent_System.DTOs;
using Car_Rent_System.Models;
using Car_Rent_System.ViewModels.Booking;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Car_Rent_System.Interfaces
{
    public interface IAdminRepository
    {
        // Dashboard methods
        Task<ApplicationUser?> GetUserByIdAsync(string userId);
        Task<IList<string>> GetUserRolesAsync(ApplicationUser user);
        Task<(int TotalCars, int AvailableCars, int TotalBookings, decimal TotalRevenue)> GetDashboardStatsAsync();
        Task<IEnumerable<BookingViewModel>> GetRecentBookingsAsync(int count);
        
        // User management methods
        Task<IEnumerable<UserDto>> GetPendingUsersAsync();
        Task<IEnumerable<UserDto>> GetAllApprovedUsersAsync();
        Task<bool> ApproveUserAsync(string userId);
        Task<bool> RejectUserAsync(string userId);
        Task<bool> UpdateUserRoleAsync(ApplicationUser user, string newRole);
        
        // Car management methods
        Task<IEnumerable<CarDto>> GetAllCarsAsync();
        Task<bool> CreateCarAsync(CarDto carDto, string imageUrl, string adminUserId);
        Task<CarDto?> GetCarByIdAsync(int carId);
        Task<bool> UpdateCarAsync(CarDto carDto);
        Task<bool> DeleteCarAsync(int carId);
        Task<bool> ToggleCarAvailabilityAsync(int carId);
        
        // Booking management methods
        Task<IEnumerable<BookingViewModel>> GetAllBookingsAsync();
        Task<bool> UpdateBookingStatusAsync(int bookingId, string status);
        Task<bool> ProcessRentAsync(int bookingId, int odometerStartReading, string paymentMethod, decimal partialPayment);
        Task<bool> ProcessReturnAsync(int bookingId, DateTime returnDate, int odometerEndReading, string paymentStatus);
        
        // Admin password reset
        Task<bool> ResetAdminPasswordAsync(string adminEmail, string newPassword);
        
        // User management methods
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
        Task<bool> UpdateUserAsync(ApplicationUser user);
        Task<bool> DeleteUserAsync(string userId);
        
        // Database management methods
        Task<bool> ClearAllCarsAsync();
    }
}
