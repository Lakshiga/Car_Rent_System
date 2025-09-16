using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Car_Rent_System.Data;
using Car_Rent_System.DTOs;
using Car_Rent_System.Enums;
using Car_Rent_System.Interfaces;
using Car_Rent_System.Models;
using Car_Rent_System.ViewModels.Car;
using Car_Rent_System.ViewModels.Booking;
using Microsoft.EntityFrameworkCore;

namespace Car_Rent_System.Services
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public BookingService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // ===== ADMIN / STAFF =====

        public async Task<int> GetTotalBookingsAsync(CancellationToken cancellationToken = default)
            => await _context.Bookings.CountAsync(cancellationToken);

        public async Task<decimal> GetTotalRevenueAsync(CancellationToken cancellationToken = default)
            => await _context.Bookings.SumAsync(b => b.TotalCost, cancellationToken);

        public async Task<IEnumerable<BookingViewModel>> GetRecentBookingsAsync(int count, CancellationToken cancellationToken = default)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Car)
                .OrderByDescending(b => b.BookingDate)
                .Take(count)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<BookingViewModel>>(bookings);
        }

        public async Task<bool> CreateBookingAsync(BookingViewModel booking, string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId))
                return false;

            var bookingModel = _mapper.Map<Booking>(booking);
            bookingModel.CustomerID = userId;
            bookingModel.BookingDate = DateTime.UtcNow;
            bookingModel.BookingStatus = BookingStatus.Pending;

            await _context.Bookings.AddAsync(bookingModel, cancellationToken);
            var result = await _context.SaveChangesAsync(cancellationToken);
            return result > 0;
        }

        public async Task<IEnumerable<BookingViewModel>> GetAllBookingsAsync(CancellationToken cancellationToken = default)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Car)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<BookingViewModel>>(bookings);
        }

        // ===== CUSTOMER =====

        public async Task<int> GetCustomerBookingsCountAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId))
                return 0;

            return await _context.Bookings.CountAsync(b => b.CustomerID == userId, cancellationToken);
        }

        public async Task<decimal> GetCustomerTotalSpentAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId))
                return 0;

            return await _context.Bookings
                .Where(b => b.CustomerID == userId)
                .SumAsync(b => b.TotalCost, cancellationToken);
        }

        public async Task<IEnumerable<BookingViewModel>> GetCustomerRecentBookingsAsync(string userId, int count, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId))
                return new List<BookingViewModel>();

            var bookings = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Car)
                .Where(b => b.CustomerID == userId)
                .OrderByDescending(b => b.BookingDate)
                .Take(count)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<BookingViewModel>>(bookings);
        }

        public async Task<IEnumerable<BookingViewModel>> GetCustomerAllBookingsAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId))
                return new List<BookingViewModel>();

            var bookings = await _context.Bookings
                .Include(b => b.Car)
                .Where(b => b.CustomerID == userId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<BookingViewModel>>(bookings);
        }

        // ===== BUSINESS LOGIC =====

        public async Task<bool> HasBookingConflictAsync(int vehicleId, DateTime pickupDate, DateTime returnDate, CancellationToken cancellationToken = default)
        {
            return await _context.Bookings.AnyAsync(b =>
                b.CarID == vehicleId &&
                b.BookingStatus == BookingStatus.Confirmed &&
                (
                    (pickupDate >= b.PickupDate && pickupDate < b.ReturnDate) ||
                    (returnDate > b.PickupDate && returnDate <= b.ReturnDate) ||
                    (pickupDate <= b.PickupDate && returnDate >= b.ReturnDate)
                ), cancellationToken);
        }

        public async Task<bool> CancelBookingAsync(int bookingId, string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId))
                return false;

            var booking = await _context.Bookings
                .Include(b => b.Car)
                .FirstOrDefaultAsync(b => b.BookingID == bookingId && b.CustomerID == userId, cancellationToken);

            if (booking != null && booking.BookingStatus == BookingStatus.Confirmed && booking.PickupDate > DateTime.Today)
            {
                booking.BookingStatus = BookingStatus.Cancelled;
                booking.Car.IsAvailable = true;

                _context.Bookings.Update(booking);
                _context.Cars.Update(booking.Car);
                var result = await _context.SaveChangesAsync(cancellationToken);
                return result > 0;
            }

            return false;
        }

        // ===== ANALYTICS =====

        public async Task<List<MonthlySpendingViewModel>> GetCustomerMonthlySpendingAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId))
                return new List<MonthlySpendingViewModel>();

            var data = await _context.Bookings
                .Where(b => b.CustomerID == userId && b.BookingStatus == BookingStatus.Confirmed)
                .GroupBy(b => new { b.BookingDate.Year, b.BookingDate.Month })
                .Select(g => new MonthlySpendingViewModel
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalSpent = g.Sum(b => b.TotalCost)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .Take(6)
                .ToListAsync(cancellationToken);

            return data;
        }

        public async Task<List<CarPreferenceViewModel>> GetCustomerCarPreferencesAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId))
                return new List<CarPreferenceViewModel>();

            var data = await _context.Bookings
                .Where(b => b.CustomerID == userId && b.BookingStatus == BookingStatus.Confirmed)
                .Include(b => b.Car)
                .GroupBy(b => new { b.Car.Make, b.Car.CarModel })
                .Select(g => new CarPreferenceViewModel
                {
                    Make = g.Key.Make,
                    Model = g.Key.CarModel,
                    BookingCount = g.Count(),
                    TotalSpent = g.Sum(b => b.TotalCost)
                })
                .OrderByDescending(x => x.BookingCount)
                .Take(5)
                .ToListAsync(cancellationToken);

            return data;
        }

        // ===== UTILITY =====

        public async Task<BookingViewModel?> GetBookingByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var booking = await _context.Bookings
                .Include(b => b.Car)
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(b => b.BookingID == id, cancellationToken);

            return booking == null ? null : _mapper.Map<BookingViewModel>(booking);
        }

        public async Task<bool> UpdateBookingAsync(BookingViewModel booking, CancellationToken cancellationToken = default)
        {
            var existing = await _context.Bookings.FindAsync(booking.Id);
            if (existing == null) return false;

            _mapper.Map(booking, existing);
            _context.Bookings.Update(existing);
            var result = await _context.SaveChangesAsync(cancellationToken);
            return result > 0;
        }

        public async Task<decimal> CalculateBookingTotalAsync(int vehicleId, DateTime pickup, DateTime dropoff, bool withDriver, CancellationToken cancellationToken = default)
        {
            var car = await _context.Cars.FindAsync(vehicleId, cancellationToken);
            if (car == null) return 0;

            var totalDays = (dropoff.Date - pickup.Date).Days;
            if (totalDays <= 0) totalDays = 1;

            decimal total = totalDays * car.DailyRate;

            if (withDriver)
            {
                total += totalDays * 50; // Example: $50/day for driver
            }

            return total;
        }
    }
}