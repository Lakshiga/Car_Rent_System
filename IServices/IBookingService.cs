using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Car_Rent_System.DTOs;
using Car_Rent_System.Models;
using Car_Rent_System.ViewModels.Booking; // 👈 Use ViewModels for MVC

namespace Car_Rent_System.Interfaces
{
    public interface IBookingService
    {
        // ===== ADMIN / STAFF =====
        Task<int> GetTotalBookingsAsync(CancellationToken cancellationToken = default);
        Task<decimal> GetTotalRevenueAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<BookingViewModel>> GetRecentBookingsAsync(int count, CancellationToken cancellationToken = default);
        Task<IEnumerable<BookingViewModel>> GetAllBookingsAsync(CancellationToken cancellationToken = default);

        // ===== CUSTOMER =====
        Task<int> GetCustomerBookingsCountAsync(string userId, CancellationToken cancellationToken = default);
        Task<decimal> GetCustomerTotalSpentAsync(string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<BookingViewModel>> GetCustomerRecentBookingsAsync(string userId, int count, CancellationToken cancellationToken = default);
        Task<IEnumerable<BookingViewModel>> GetCustomerAllBookingsAsync(string userId, CancellationToken cancellationToken = default);

        // ===== BUSINESS LOGIC =====
        Task<bool> HasBookingConflictAsync(int vehicleId, DateTime pickupDate, DateTime returnDate, CancellationToken cancellationToken = default);
        Task<bool> CreateBookingAsync(BookingViewModel booking, string userId, CancellationToken cancellationToken = default);
        Task<bool> CancelBookingAsync(int bookingId, string userId, CancellationToken cancellationToken = default);

        // ===== ANALYTICS =====
        Task<List<MonthlySpendingViewModel>> GetCustomerMonthlySpendingAsync(string userId, CancellationToken cancellationToken = default);
        Task<List<CarPreferenceViewModel>> GetCustomerCarPreferencesAsync(string userId, CancellationToken cancellationToken = default);

        // ===== UTILITY =====
        Task<BookingViewModel?> GetBookingByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> UpdateBookingAsync(BookingViewModel booking, CancellationToken cancellationToken = default);
        Task<decimal> CalculateBookingTotalAsync(int vehicleId, DateTime pickup, DateTime dropoff, bool withDriver, CancellationToken cancellationToken = default);
    }
}