using Car_Rent_System.DTOs;
using Car_Rent_System.Models;
using BookingViewModel = Car_Rent_System.ViewModels.Booking.BookingViewModel;
using CustomerBookingViewModel = Car_Rent_System.ViewModels.Customer.BookingViewModel;
using DashboardViewModel = Car_Rent_System.ViewModels.Customer.DashboardViewModel;
using MonthlySpendingViewModel = Car_Rent_System.ViewModels.Booking.MonthlySpendingViewModel;

namespace Car_Rent_System.Interfaces
{
    public interface ICustomerService
    {
        // Dashboard methods
        Task<DashboardViewModel> GetCustomerDashboardAsync(string customerId);
        Task<int> GetCustomerBookingsCountAsync(string customerId);
        Task<decimal> GetCustomerTotalSpentAsync(string customerId);
        Task<IEnumerable<BookingViewModel>> GetCustomerRecentBookingsAsync(string customerId, int count);
        Task<IEnumerable<MonthlySpendingViewModel>> GetCustomerMonthlySpendingAsync(string customerId);
        Task<IEnumerable<CarPreferenceDto>> GetCustomerCarPreferencesAsync(string customerId);

        // Car browsing and booking methods
        Task<IEnumerable<CarDto>> SearchCarsAsync(string searchTerm, string carType, string fuelType, decimal? maxPrice);
        Task<IEnumerable<string>> GetDistinctCarTypesAsync();
        Task<IEnumerable<string>> GetDistinctFuelTypesAsync();
        Task<CarDto?> GetCarByIdAsync(int carId);
        Task<bool> IsCarAvailableAsync(int carId);
        Task<bool> HasBookingConflictAsync(int carId, DateTime pickupDate, DateTime returnDate);

        // Booking management methods
        Task<bool> CreateBookingAsync(BookingViewModel bookingViewModel, string customerId);
        Task<decimal> CalculateBookingCostAsync(BookingViewModel bookingViewModel);
        Task<IEnumerable<BookingViewModel>> GetCustomerAllBookingsAsync(string customerId);
        Task<BookingViewModel?> GetBookingByIdAsync(int bookingId, string customerId);
        Task<bool> CancelBookingAsync(int bookingId, string customerId);
        Task<bool> UpdateBookingAsync(BookingViewModel bookingViewModel, string customerId);

        // Payment methods
        Task<string> CreateCheckoutSessionAsync(BookingDto bookingDto, string carName);
        Task<bool> ProcessPaymentSuccessAsync(string sessionId, string customerId);
        Task<bool> ProcessPaymentFailureAsync(string sessionId, string customerId);

        // Profile management methods
        Task<ProfileDto?> GetCustomerProfileAsync(string customerId);
        Task<bool> UpdateCustomerProfileAsync(ProfileDto profileDto, string customerId);
        Task<bool> ChangePasswordAsync(string customerId, string currentPassword, string newPassword);

        // Preferences and settings
        Task<bool> SaveCarPreferencesAsync(string customerId, CarPreferenceDto preferences);
        Task<CarPreferenceDto?> GetCarPreferencesAsync(string customerId);
        Task<bool> UpdateNotificationSettingsAsync(string customerId, bool emailNotifications, bool smsNotifications);
    }
}
