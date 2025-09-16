using Car_Rent_System.Data;
using Car_Rent_System.DTOs;
using Car_Rent_System.Enums;
using Car_Rent_System.Interfaces;
using Car_Rent_System.Models;
using BookingViewModel = Car_Rent_System.ViewModels.Booking.BookingViewModel;
using CustomerBookingViewModel = Car_Rent_System.ViewModels.Customer.BookingViewModel;
using DashboardViewModel = Car_Rent_System.ViewModels.Customer.DashboardViewModel;
using MonthlySpendingViewModel = Car_Rent_System.ViewModels.Booking.MonthlySpendingViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Car_Rent_System.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomerRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Dashboard data methods
        public async Task<DashboardViewModel> GetCustomerDashboardDataAsync(string customerId)
        {
            try
            {
                var totalBookings = await GetCustomerBookingsCountAsync(customerId);
                var totalSpent = await GetCustomerTotalSpentAsync(customerId);
                var recentBookings = await GetCustomerRecentBookingsAsync(customerId, 5);
                var monthlySpending = await GetCustomerMonthlySpendingAsync(customerId);
                var carPreferences = await GetCustomerCarPreferencesAsync(customerId);

                return new DashboardViewModel
                {
                    TotalBookings = totalBookings,
                    TotalSpent = totalSpent,
                    RecentBookings = recentBookings.ToList(),
                    MonthlySpending = monthlySpending.ToList(),
                    CarPreferences = carPreferences.ToList()
                };
            }
            catch (Exception)
            {
                return new DashboardViewModel();
            }
        }

        public async Task<int> GetCustomerBookingsCountAsync(string customerId)
        {
            try
            {
                return await _context.Bookings
                    .Where(b => b.CustomerID == customerId)
                    .CountAsync();
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<decimal> GetCustomerTotalSpentAsync(string customerId)
        {
            try
            {
                return await _context.Bookings
                    .Where(b => b.CustomerID == customerId && b.BookingStatus == BookingStatus.Confirmed)
                    .SumAsync(b => b.TotalCost);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<IEnumerable<BookingViewModel>> GetCustomerRecentBookingsAsync(string customerId, int count)
        {
            try
            {
                var bookings = await _context.Bookings
                    .Include(b => b.Car)
                    .Where(b => b.CustomerID == customerId)
                    .OrderByDescending(b => b.BookingDate)
                    .Take(count)
                    .ToListAsync();

                return bookings.Select(MapToBookingViewModel);
            }
            catch (Exception)
            {
                return new List<BookingViewModel>();
            }
        }

        public async Task<IEnumerable<MonthlySpendingViewModel>> GetCustomerMonthlySpendingAsync(string customerId)
        {
            try
            {
                var monthlyData = await _context.Bookings
                    .Where(b => b.CustomerID == customerId && b.BookingStatus == BookingStatus.Confirmed)
                    .GroupBy(b => new { b.BookingDate.Year, b.BookingDate.Month })
                    .Select(g => new MonthlySpendingViewModel
                    {
                        Month = g.Key.Month,
                        Year = g.Key.Year,
                        TotalSpent = g.Sum(b => b.TotalCost)
                    })
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month)
                    .ToListAsync();

                return monthlyData;
            }
            catch (Exception)
            {
                return new List<MonthlySpendingViewModel>();
            }
        }

        public async Task<IEnumerable<CarPreferenceDto>> GetCustomerCarPreferencesAsync(string customerId)
        {
            try
            {
                // This would typically come from a user preferences table
                // For now, we'll analyze booking history to determine preferences
                var bookingHistory = await _context.Bookings
                    .Include(b => b.Car)
                    .Where(b => b.CustomerID == customerId)
                    .ToListAsync();

                var preferences = bookingHistory
                    .GroupBy(b => b.Car.CarType)
                    .Select(g => new CarPreferenceDto
                    {
                        CarType = g.Key,
                        BookingCount = g.Count(),
                        TotalSpent = g.Sum(b => b.TotalCost)
                    })
                    .OrderByDescending(p => p.BookingCount)
                    .ToList();

                return preferences;
            }
            catch (Exception)
            {
                return new List<CarPreferenceDto>();
            }
        }

        // Car data methods
        public async Task<IEnumerable<CarDto>> SearchCarsAsync(string searchTerm, string carType, string fuelType, decimal? maxPrice)
        {
            try
            {
                var query = _context.Cars.AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(c => c.CarName.Contains(searchTerm) || c.CarModel.Contains(searchTerm));
                }

                if (!string.IsNullOrEmpty(carType))
                {
                    query = query.Where(c => c.CarType == carType);
                }

                if (!string.IsNullOrEmpty(fuelType))
                {
                    query = query.Where(c => c.FuelType == fuelType);
                }

                if (maxPrice.HasValue)
                {
                    query = query.Where(c => c.DailyRate <= maxPrice.Value);
                }

                var cars = await query
                    .Where(c => c.IsAvailable)
                    .OrderByDescending(c => c.DateAdded)
                    .ToListAsync();


                return cars.Select(MapToCarDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SearchCarsAsync error: {ex.Message}");
                return new List<CarDto>();
            }
        }

        public async Task<IEnumerable<string>> GetDistinctCarTypesAsync()
        {
            try
            {
                return await _context.Cars
                    .Where(c => c.CarType != null && c.IsAvailable)
                    .Select(c => c.CarType)
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        public async Task<IEnumerable<string>> GetDistinctFuelTypesAsync()
        {
            try
            {
                return await _context.Cars
                    .Where(c => c.FuelType != null && c.IsAvailable)
                    .Select(c => c.FuelType)
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        public async Task<CarDto?> GetCarByIdAsync(int carId)
        {
            try
            {
                var car = await _context.Cars.FindAsync(carId);
                return car == null ? null : MapToCarDto(car);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> IsCarAvailableAsync(int carId)
        {
            try
            {
                var car = await _context.Cars.FindAsync(carId);
                return car != null && car.IsAvailable;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> HasBookingConflictAsync(int carId, DateTime pickupDate, DateTime returnDate)
        {
            try
            {
                return await _context.Bookings
                    .AnyAsync(b => b.CarID == carId &&
                                  b.BookingStatus == BookingStatus.Confirmed &&
                                  ((pickupDate >= b.PickupDate && pickupDate <= b.ReturnDate) ||
                                   (returnDate >= b.PickupDate && returnDate <= b.ReturnDate) ||
                                   (pickupDate <= b.PickupDate && returnDate >= b.ReturnDate)));
            }
            catch (Exception)
            {
                return true; // Assume conflict if error occurs
            }
        }

        // Booking CRUD methods
        public async Task<bool> CreateBookingAsync(Booking booking)
        {
            try
            {
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IEnumerable<BookingViewModel>> GetCustomerAllBookingsAsync(string customerId)
        {
            try
            {
                var bookings = await _context.Bookings
                    .Include(b => b.Car)
                    .Where(b => b.CustomerID == customerId)
                    .OrderByDescending(b => b.BookingDate)
                    .ToListAsync();

                return bookings.Select(MapToBookingViewModel);
            }
            catch (Exception)
            {
                return new List<BookingViewModel>();
            }
        }

        public async Task<BookingViewModel?> GetBookingByIdAsync(int bookingId, string customerId)
        {
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.Car)
                    .FirstOrDefaultAsync(b => b.BookingID == bookingId && b.CustomerID == customerId);

                return booking == null ? null : MapToBookingViewModel(booking);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> CancelBookingAsync(int bookingId, string customerId)
        {
            try
            {
                var booking = await _context.Bookings
                    .FirstOrDefaultAsync(b => b.BookingID == bookingId && b.CustomerID == customerId);

                if (booking == null || booking.BookingStatus != BookingStatus.Confirmed)
                    return false;

                // Check if cancellation is allowed (e.g., not within 24 hours of pickup)
                if (booking.PickupDate <= DateTime.Now.AddDays(1))
                    return false;

                booking.BookingStatus = BookingStatus.Cancelled;
                _context.Bookings.Update(booking);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateBookingAsync(Booking booking)
        {
            try
            {
                _context.Bookings.Update(booking);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // User profile methods
        public async Task<ApplicationUser?> GetCustomerByIdAsync(string customerId)
        {
            try
            {
                return await _userManager.FindByIdAsync(customerId);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> UpdateCustomerProfileAsync(ApplicationUser customer)
        {
            try
            {
                var result = await _userManager.UpdateAsync(customer);
                return result.Succeeded;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ChangeCustomerPasswordAsync(ApplicationUser customer, string newPassword)
        {
            try
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(customer);
                var result = await _userManager.ResetPasswordAsync(customer, token, newPassword);
                return result.Succeeded;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Preferences and settings methods
        public async Task<bool> SaveCarPreferencesAsync(string customerId, CarPreferenceDto preferences)
        {
            try
            {
                // This would typically save to a user preferences table
                // For now, we'll just return true as a placeholder
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<CarPreferenceDto?> GetCarPreferencesAsync(string customerId)
        {
            try
            {
                // This would typically retrieve from a user preferences table
                // For now, we'll return null as a placeholder
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> UpdateNotificationSettingsAsync(string customerId, bool emailNotifications, bool smsNotifications)
        {
            try
            {
                // This would typically update notification settings in a user preferences table
                // For now, we'll just return true as a placeholder
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Payment tracking methods
        public async Task<bool> CreatePaymentRecordAsync(Payment payment)
        {
            try
            {
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<Payment?> GetPaymentBySessionIdAsync(string sessionId)
        {
            try
            {
                return await _context.Payments
                    .FirstOrDefaultAsync(p => p.StripeSessionId == sessionId);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> UpdatePaymentStatusAsync(string sessionId, string status)
        {
            try
            {
                var payment = await GetPaymentBySessionIdAsync(sessionId);
                if (payment == null) return false;

                payment.Status = status;
                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Mapping methods
        private CarDto MapToCarDto(Car car)
        {
            return new CarDto
            {
                CarID = car.Id,
                CarName = car.CarName,
                CarModel = car.CarModel,
                CarType = car.CarType,
                DailyRate = car.DailyRate,
                FuelType = car.FuelType,
                Transmission = car.Transmission,
                SeatingCapacity = car.SeatingCapacity,
                Mileage = (decimal?)car.Mileage,
                Description = car.Description,
                ImageUrl = car.ImageUrl,
                IsAvailable = car.IsAvailable,
                DateAdded = car.DateAdded
            };
        }

        private BookingViewModel MapToBookingViewModel(Booking booking)
        {
            return new BookingViewModel
            {
                Id = booking.BookingID,
                VehicleId = booking.CarID,
                PickupDate = booking.PickupDate,
                ReturnDate = booking.ReturnDate,
                TotalAmount = booking.TotalCost,
                TotalCost = booking.TotalCost,
                Status = booking.BookingStatus.ToString(),
                BookingStatus = booking.BookingStatus,
                PickupLocation = booking.PickupLocation,
                ReturnLocation = booking.ReturnLocation,
                SpecialRequirements = booking.SpecialRequirements,
                WithDriver = booking.WithDriver,
                CarName = booking.Car?.CarName,
                UserName = booking.Customer?.FullName
            };
        }
    }
}
