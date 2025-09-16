using Car_Rent_System.DTOs;
using Car_Rent_System.Enums;
using Car_Rent_System.Interfaces;
using Car_Rent_System.Models;
using BookingViewModel = Car_Rent_System.ViewModels.Booking.BookingViewModel;
using CustomerBookingViewModel = Car_Rent_System.ViewModels.Customer.BookingViewModel;
using DashboardViewModel = Car_Rent_System.ViewModels.Customer.DashboardViewModel;
using MonthlySpendingViewModel = Car_Rent_System.ViewModels.Booking.MonthlySpendingViewModel;
using Microsoft.AspNetCore.Identity;

namespace Car_Rent_System.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomerService(ICustomerRepository customerRepository, UserManager<ApplicationUser> userManager)
        {
            _customerRepository = customerRepository;
            _userManager = userManager;
        }

        // Dashboard methods
        public async Task<DashboardViewModel> GetCustomerDashboardAsync(string customerId)
        {
            try
            {
                return await _customerRepository.GetCustomerDashboardDataAsync(customerId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetCustomerDashboardAsync error: {ex.Message}");
                return new DashboardViewModel();
            }
        }

        public async Task<int> GetCustomerBookingsCountAsync(string customerId)
        {
            try
            {
                return await _customerRepository.GetCustomerBookingsCountAsync(customerId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetCustomerBookingsCountAsync error: {ex.Message}");
                return 0;
            }
        }

        public async Task<decimal> GetCustomerTotalSpentAsync(string customerId)
        {
            try
            {
                return await _customerRepository.GetCustomerTotalSpentAsync(customerId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetCustomerTotalSpentAsync error: {ex.Message}");
                return 0;
            }
        }

        public async Task<IEnumerable<BookingViewModel>> GetCustomerRecentBookingsAsync(string customerId, int count)
        {
            try
            {
                return await _customerRepository.GetCustomerRecentBookingsAsync(customerId, count);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetCustomerRecentBookingsAsync error: {ex.Message}");
                return new List<BookingViewModel>();
            }
        }

        public async Task<IEnumerable<MonthlySpendingViewModel>> GetCustomerMonthlySpendingAsync(string customerId)
        {
            try
            {
                return await _customerRepository.GetCustomerMonthlySpendingAsync(customerId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetCustomerMonthlySpendingAsync error: {ex.Message}");
                return new List<MonthlySpendingViewModel>();
            }
        }

        public async Task<IEnumerable<CarPreferenceDto>> GetCustomerCarPreferencesAsync(string customerId)
        {
            try
            {
                return await _customerRepository.GetCustomerCarPreferencesAsync(customerId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetCustomerCarPreferencesAsync error: {ex.Message}");
                return new List<CarPreferenceDto>();
            }
        }

        // Car browsing and booking methods
        public async Task<IEnumerable<CarDto>> SearchCarsAsync(string searchTerm, string carType, string fuelType, decimal? maxPrice)
        {
            try
            {
                return await _customerRepository.SearchCarsAsync(searchTerm, carType, fuelType, maxPrice);
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
                return await _customerRepository.GetDistinctCarTypesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetDistinctCarTypesAsync error: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<IEnumerable<string>> GetDistinctFuelTypesAsync()
        {
            try
            {
                return await _customerRepository.GetDistinctFuelTypesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetDistinctFuelTypesAsync error: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<CarDto?> GetCarByIdAsync(int carId)
        {
            try
            {
                return await _customerRepository.GetCarByIdAsync(carId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetCarByIdAsync error: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> IsCarAvailableAsync(int carId)
        {
            try
            {
                return await _customerRepository.IsCarAvailableAsync(carId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ IsCarAvailableAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> HasBookingConflictAsync(int carId, DateTime pickupDate, DateTime returnDate)
        {
            try
            {
                return await _customerRepository.HasBookingConflictAsync(carId, pickupDate, returnDate);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ HasBookingConflictAsync error: {ex.Message}");
                return true; // Assume conflict if error occurs
            }
        }

        // Booking management methods
        public async Task<bool> CreateBookingAsync(BookingViewModel bookingViewModel, string customerId)
        {
            try
            {
                // Validate booking data
                if (bookingViewModel.PickupDate < DateTime.Today)
                {
                    Console.WriteLine("❌ Pickup date cannot be in the past");
                    return false;
                }

                if (bookingViewModel.ReturnDate <= bookingViewModel.PickupDate)
                {
                    Console.WriteLine("❌ Return date must be after pickup date");
                    return false;
                }

                // Validate odometer readings
                if (bookingViewModel.OdometerStartReading.HasValue && bookingViewModel.OdometerEndReading.HasValue)
                {
                    if (bookingViewModel.OdometerEndReading <= bookingViewModel.OdometerStartReading)
                    {
                        Console.WriteLine("❌ End odometer reading must be greater than start reading");
                        return false;
                    }
                }

                // Check for conflicts
                var hasConflict = await HasBookingConflictAsync(bookingViewModel.VehicleId, bookingViewModel.PickupDate, bookingViewModel.ReturnDate);
                if (hasConflict)
                {
                    Console.WriteLine("❌ Booking conflict detected");
                    return false;
                }

                // Calculate pricing based on distance or daily rate
                var totalCost = await CalculateBookingCostAsync(bookingViewModel);

                // Create booking entity
                var booking = new Booking
                {
                    CarID = bookingViewModel.VehicleId,
                    CustomerID = customerId,
                    PickupDate = bookingViewModel.PickupDate,
                    ReturnDate = bookingViewModel.ReturnDate,
                    TotalCost = totalCost,
                    BookingStatus = BookingStatus.Pending,
                    PickupLocation = bookingViewModel.PickupLocation,
                    ReturnLocation = bookingViewModel.ReturnLocation,
                    SpecialRequirements = bookingViewModel.SpecialRequirements,
                    WithDriver = bookingViewModel.WithDriver ?? false,
                    BookingDate = DateTime.UtcNow,
                    StripeSessionId = bookingViewModel.StripeSessionId,
                    LicenseNumber = bookingViewModel.LicenseNumber,
                    NICNumber = bookingViewModel.NICNumber,
                    DocumentImageUrl = bookingViewModel.DocumentImageUrl,
                    OdometerStartReading = bookingViewModel.OdometerStartReading,
                    OdometerEndReading = bookingViewModel.OdometerEndReading,
                    DistanceTraveled = bookingViewModel.DistanceTraveled,
                    PerKilometerRate = bookingViewModel.PerKilometerRate,
                    AdvancePayment = bookingViewModel.AdvancePayment,
                    RemainingAmount = bookingViewModel.RemainingAmount
                };

                return await _customerRepository.CreateBookingAsync(booking);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ CreateBookingAsync error: {ex.Message}");
                return false;
            }
        }

        // New method to calculate booking cost based on distance or daily rate
        public async Task<decimal> CalculateBookingCostAsync(BookingViewModel bookingViewModel)
        {
            try
            {
                // Get car details to determine pricing method
                var car = await GetCarByIdAsync(bookingViewModel.VehicleId);
                if (car == null)
                {
                    Console.WriteLine("❌ Car not found for cost calculation");
                    return 0;
                }

                // If odometer readings are provided, calculate based on distance
                if (bookingViewModel.OdometerStartReading.HasValue && bookingViewModel.OdometerEndReading.HasValue)
                {
                    var distance = bookingViewModel.OdometerEndReading.Value - bookingViewModel.OdometerStartReading.Value;
                    bookingViewModel.DistanceTraveled = distance;
                    
                    // Set per kilometer rate (you can make this configurable)
                    var perKmRate = car.PerKilometerRate ?? 2.50m; // Default rate if not set
                    bookingViewModel.PerKilometerRate = perKmRate;
                    
                    var totalCost = distance * perKmRate;
                    bookingViewModel.TotalAmount = totalCost;
                    bookingViewModel.TotalCost = totalCost;
                    
                    // Calculate advance payment (50% of total)
                    bookingViewModel.AdvancePayment = totalCost * 0.5m;
                    bookingViewModel.RemainingAmount = totalCost - bookingViewModel.AdvancePayment.Value;
                    
                    return totalCost;
                }
                else
                {
                    // Fallback to daily rate calculation
                    var totalDays = (bookingViewModel.ReturnDate - bookingViewModel.PickupDate).Days + 1;
                    var totalCost = car.DailyRate * totalDays;
                    bookingViewModel.TotalAmount = totalCost;
                    bookingViewModel.TotalCost = totalCost;
                    
                    // Calculate advance payment (50% of total)
                    bookingViewModel.AdvancePayment = totalCost * 0.5m;
                    bookingViewModel.RemainingAmount = totalCost - bookingViewModel.AdvancePayment.Value;
                    
                    return totalCost;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ CalculateBookingCostAsync error: {ex.Message}");
                return 0;
            }
        }

        public async Task<IEnumerable<BookingViewModel>> GetCustomerAllBookingsAsync(string customerId)
        {
            try
            {
                return await _customerRepository.GetCustomerAllBookingsAsync(customerId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetCustomerAllBookingsAsync error: {ex.Message}");
                return new List<BookingViewModel>();
            }
        }

        public async Task<BookingViewModel?> GetBookingByIdAsync(int bookingId, string customerId)
        {
            try
            {
                return await _customerRepository.GetBookingByIdAsync(bookingId, customerId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetBookingByIdAsync error: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CancelBookingAsync(int bookingId, string customerId)
        {
            try
            {
                return await _customerRepository.CancelBookingAsync(bookingId, customerId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ CancelBookingAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateBookingAsync(BookingViewModel bookingViewModel, string customerId)
        {
            try
            {
                // Get existing booking
                var existingBooking = await _customerRepository.GetBookingByIdAsync(bookingViewModel.Id, customerId);
                if (existingBooking == null)
                {
                    Console.WriteLine("❌ Booking not found");
                    return false;
                }

                // Check if booking can be updated (e.g., not confirmed yet)
                if (existingBooking.BookingStatus == BookingStatus.Confirmed)
                {
                    Console.WriteLine("❌ Cannot update confirmed booking");
                    return false;
                }

                // Create updated booking entity
                var booking = new Booking
                {
                    BookingID = bookingViewModel.Id,
                    CarID = bookingViewModel.VehicleId,
                    CustomerID = customerId,
                    PickupDate = bookingViewModel.PickupDate,
                    ReturnDate = bookingViewModel.ReturnDate,
                    TotalCost = bookingViewModel.TotalAmount,
                    BookingStatus = bookingViewModel.BookingStatus,
                    PickupLocation = bookingViewModel.PickupLocation,
                    ReturnLocation = bookingViewModel.ReturnLocation,
                    SpecialRequirements = bookingViewModel.SpecialRequirements,
                    WithDriver = bookingViewModel.WithDriver ?? false,
                    BookingDate = existingBooking.BookingDate,
                    StripeSessionId = bookingViewModel.StripeSessionId
                };

                return await _customerRepository.UpdateBookingAsync(booking);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ UpdateBookingAsync error: {ex.Message}");
                return false;
            }
        }

        // Payment methods
        public async Task<string> CreateCheckoutSessionAsync(BookingDto bookingDto, string carName)
        {
            try
            {
                // This would typically integrate with Stripe service
                // For now, return a placeholder session ID
                return $"cs_test_{Guid.NewGuid():N}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ CreateCheckoutSessionAsync error: {ex.Message}");
                return string.Empty;
            }
        }

        public async Task<bool> ProcessPaymentSuccessAsync(string sessionId, string customerId)
        {
            try
            {
                // Update booking status to confirmed
                var payment = await _customerRepository.GetPaymentBySessionIdAsync(sessionId);
                if (payment == null)
                {
                    Console.WriteLine("❌ Payment record not found");
                    return false;
                }

                // Update payment status
                await _customerRepository.UpdatePaymentStatusAsync(sessionId, "succeeded");

                // Update booking status
                var booking = await _customerRepository.GetBookingByIdAsync(payment.BookingId, customerId);
                if (booking != null)
                {
                    booking.BookingStatus = BookingStatus.Confirmed;
                    await _customerRepository.UpdateBookingAsync(MapToBooking(booking));
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ProcessPaymentSuccessAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ProcessPaymentFailureAsync(string sessionId, string customerId)
        {
            try
            {
                // Update payment status to failed
                await _customerRepository.UpdatePaymentStatusAsync(sessionId, "failed");

                // Update booking status to cancelled
                var payment = await _customerRepository.GetPaymentBySessionIdAsync(sessionId);
                if (payment != null)
                {
                    var booking = await _customerRepository.GetBookingByIdAsync(payment.BookingId, customerId);
                    if (booking != null)
                    {
                        booking.BookingStatus = BookingStatus.Cancelled;
                        await _customerRepository.UpdateBookingAsync(MapToBooking(booking));
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ProcessPaymentFailureAsync error: {ex.Message}");
                return false;
            }
        }

        // Profile management methods
        public async Task<ProfileDto?> GetCustomerProfileAsync(string customerId)
        {
            try
            {
                var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
                if (customer == null) return null;

                return new ProfileDto
                {
                    Id = customer.Id,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Email = customer.Email,
                    PhoneNumber = customer.PhoneNumber,
                    Address = customer.Address,
                    DateOfBirth = customer.DateOfBirth,
                    LicenseNumber = customer.LicenseNumber,
                    CompanyName = customer.CompanyName
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetCustomerProfileAsync error: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateCustomerProfileAsync(ProfileDto profileDto, string customerId)
        {
            try
            {
                var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
                if (customer == null) return false;

                // Update customer properties
                customer.FirstName = profileDto.FirstName;
                customer.LastName = profileDto.LastName;
                customer.Email = profileDto.Email;
                customer.PhoneNumber = profileDto.PhoneNumber;
                customer.Address = profileDto.Address;
                customer.DateOfBirth = profileDto.DateOfBirth;
                customer.LicenseNumber = profileDto.LicenseNumber;
                customer.CompanyName = profileDto.CompanyName;

                return await _customerRepository.UpdateCustomerProfileAsync(customer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ UpdateCustomerProfileAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(string customerId, string currentPassword, string newPassword)
        {
            try
            {
                var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
                if (customer == null) return false;

                // Verify current password
                var isValidPassword = await _userManager.CheckPasswordAsync(customer, currentPassword);
                if (!isValidPassword)
                {
                    Console.WriteLine("❌ Current password is incorrect");
                    return false;
                }

                return await _customerRepository.ChangeCustomerPasswordAsync(customer, newPassword);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ChangePasswordAsync error: {ex.Message}");
                return false;
            }
        }

        // Preferences and settings
        public async Task<bool> SaveCarPreferencesAsync(string customerId, CarPreferenceDto preferences)
        {
            try
            {
                return await _customerRepository.SaveCarPreferencesAsync(customerId, preferences);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SaveCarPreferencesAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<CarPreferenceDto?> GetCarPreferencesAsync(string customerId)
        {
            try
            {
                return await _customerRepository.GetCarPreferencesAsync(customerId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetCarPreferencesAsync error: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateNotificationSettingsAsync(string customerId, bool emailNotifications, bool smsNotifications)
        {
            try
            {
                return await _customerRepository.UpdateNotificationSettingsAsync(customerId, emailNotifications, smsNotifications);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ UpdateNotificationSettingsAsync error: {ex.Message}");
                return false;
            }
        }

        // Helper methods
        private Booking MapToBooking(BookingViewModel viewModel)
        {
            return new Booking
            {
                BookingID = viewModel.Id,
                CarID = viewModel.VehicleId,
                PickupDate = viewModel.PickupDate,
                ReturnDate = viewModel.ReturnDate,
                TotalCost = viewModel.TotalAmount,
                BookingStatus = viewModel.BookingStatus,
                PickupLocation = viewModel.PickupLocation,
                ReturnLocation = viewModel.ReturnLocation,
                SpecialRequirements = viewModel.SpecialRequirements,
                WithDriver = viewModel.WithDriver ?? false,
                BookingDate = viewModel.BookingDate,
                StripeSessionId = viewModel.StripeSessionId
            };
        }
    }
}
