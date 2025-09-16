using Car_Rent_System.DTOs;
using Car_Rent_System.Models;
using BookingViewModel = Car_Rent_System.ViewModels.Booking.BookingViewModel;
using DashboardViewModel = Car_Rent_System.ViewModels.Customer.DashboardViewModel;
using MonthlySpendingViewModel = Car_Rent_System.ViewModels.Booking.MonthlySpendingViewModel;

namespace Car_Rent_System.Interfaces
{
    public interface ICustomerRepository
    {
        // Dashboard data methods
        Task<DashboardViewModel> GetCustomerDashboardDataAsync(string customerId);
        Task<int> GetCustomerBookingsCountAsync(string customerId);
        Task<decimal> GetCustomerTotalSpentAsync(string customerId);
        Task<IEnumerable<BookingViewModel>> GetCustomerRecentBookingsAsync(string customerId, int count);
        Task<IEnumerable<MonthlySpendingViewModel>> GetCustomerMonthlySpendingAsync(string customerId);
        Task<IEnumerable<CarPreferenceDto>> GetCustomerCarPreferencesAsync(string customerId);

        // Car data methods
        Task<IEnumerable<CarDto>> SearchCarsAsync(string searchTerm, string carType, string fuelType, decimal? maxPrice);
        Task<IEnumerable<string>> GetDistinctCarTypesAsync();
        Task<IEnumerable<string>> GetDistinctFuelTypesAsync();
        Task<CarDto?> GetCarByIdAsync(int carId);
        Task<bool> IsCarAvailableAsync(int carId);
        Task<bool> HasBookingConflictAsync(int carId, DateTime pickupDate, DateTime returnDate);

        // Booking CRUD methods
        Task<bool> CreateBookingAsync(Booking booking);
        Task<IEnumerable<BookingViewModel>> GetCustomerAllBookingsAsync(string customerId);
        Task<BookingViewModel?> GetBookingByIdAsync(int bookingId, string customerId);
        Task<bool> CancelBookingAsync(int bookingId, string customerId);
        Task<bool> UpdateBookingAsync(Booking booking);

        // User profile methods
        Task<ApplicationUser?> GetCustomerByIdAsync(string customerId);
        Task<bool> UpdateCustomerProfileAsync(ApplicationUser customer);
        Task<bool> ChangeCustomerPasswordAsync(ApplicationUser customer, string newPassword);

        // Preferences and settings methods
        Task<bool> SaveCarPreferencesAsync(string customerId, CarPreferenceDto preferences);
        Task<CarPreferenceDto?> GetCarPreferencesAsync(string customerId);
        Task<bool> UpdateNotificationSettingsAsync(string customerId, bool emailNotifications, bool smsNotifications);

        // Payment tracking methods
        Task<bool> CreatePaymentRecordAsync(Payment payment);
        Task<Payment?> GetPaymentBySessionIdAsync(string sessionId);
        Task<bool> UpdatePaymentStatusAsync(string sessionId, string status);
    }
}
