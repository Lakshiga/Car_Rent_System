using Car_Rent_System.DTOs;
using Car_Rent_System.Data;
using Car_Rent_System.Enums;
using Car_Rent_System.Interfaces;
using Car_Rent_System.Models;
using Car_Rent_System.ViewModels.Booking;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Car_Rent_System.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Dashboard methods
        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<(int TotalCars, int AvailableCars, int TotalBookings, decimal TotalRevenue)> GetDashboardStatsAsync()
        {
            var totalCars = await _context.Cars.CountAsync();
            var availableCars = await _context.Cars.CountAsync(c => c.IsAvailable);
            var totalBookings = await _context.Bookings.CountAsync();
            var totalRevenue = await _context.Bookings.SumAsync(b => b.TotalCost);

            return (totalCars, availableCars, totalBookings, totalRevenue);
        }

        public async Task<IEnumerable<BookingViewModel>> GetRecentBookingsAsync(int count)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Car)
                .Include(b => b.Customer)
                .OrderByDescending(b => b.BookingDate)
                .Take(count)
                .ToListAsync();

            return bookings.Select(MapToBookingViewModel).ToList();
        }

        // User management methods
        public async Task<IEnumerable<UserDto>> GetPendingUsersAsync()
        {
            var users = await _context.Users
                .Where(u => u.VerificationStatus == VerificationStatus.Pending)
                .OrderBy(u => u.JoinDate)
                .ToListAsync();

            return users.Select(MapToUserDto);
        }

        public async Task<IEnumerable<UserDto>> GetAllApprovedUsersAsync()
        {
            var users = await _context.Users
                .Where(u => u.VerificationStatus == VerificationStatus.Approved)
                .OrderBy(u => u.UserName)
                .ToListAsync();

            return users.Select(MapToUserDto);
        }

        public async Task<bool> ApproveUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.VerificationStatus = VerificationStatus.Approved;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> RejectUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.VerificationStatus = VerificationStatus.Rejected;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> UpdateUserRoleAsync(ApplicationUser user, string newRole)
        {
            try
            {
                // Get current roles
                var currentRoles = await _userManager.GetRolesAsync(user);

                // Remove current roles
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                // Add new role
                await _userManager.AddToRoleAsync(user, newRole);

                // Update user's Role property
                user.Role = Enum.Parse<Car_Rent_System.Enums.Role>(newRole);
                await _userManager.UpdateAsync(user);

                return true;
            }
            catch
            {
                return false;
            }
        }

        // Car management methods
        public async Task<IEnumerable<CarDto>> GetAllCarsAsync()
        {
            try
            {
                var cars = await _context.Cars
                    .OrderByDescending(c => c.DateAdded)
                    .ToListAsync();

                return cars.Select(MapToCarDto);
            }
            catch (Exception)
            {
                return new List<CarDto>();
            }
        }

        public async Task<bool> CreateCarAsync(CarDto carDto, string imageUrl, string adminUserId)
        {
            try
            {
                var car = new Car
                {
                    CarName = carDto.CarName,
                    CarModel = carDto.CarModel,
                    CarType = carDto.CarType,
                    DailyRate = carDto.DailyRate,
                    FuelType = carDto.FuelType,
                    Transmission = carDto.Transmission,
                    SeatingCapacity = carDto.SeatingCapacity,
                    Mileage = (double?)carDto.Mileage,
                    Description = carDto.Description,
                    ImageUrl = imageUrl,
                    IsAvailable = carDto.IsAvailable,
                    ApplicationUserId = adminUserId,
                    DateAdded = DateTime.UtcNow
                };

                _context.Cars.Add(car);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<CarDto?> GetCarByIdAsync(int carId)
        {
            var car = await _context.Cars.FindAsync(carId);
            if (car == null) return null;

            return new CarDto
            {
                Id = car.Id,
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
                IsAvailable = car.IsAvailable
            };
        }

        public async Task<bool> UpdateCarAsync(CarDto carDto)
        {
            try
            {
                var car = await _context.Cars.FindAsync(carDto.Id);
                if (car == null) return false;

                car.CarName = carDto.CarName;
                car.CarModel = carDto.CarModel;
                car.CarType = carDto.CarType;
                car.DailyRate = carDto.DailyRate;
                car.FuelType = carDto.FuelType;
                car.Transmission = carDto.Transmission;
                car.SeatingCapacity = carDto.SeatingCapacity;
                car.Mileage = (double?)carDto.Mileage;
                car.Description = carDto.Description;
                car.ImageUrl = carDto.ImageUrl;
                car.IsAvailable = carDto.IsAvailable;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteCarAsync(int carId)
        {
            try
            {
                var car = await _context.Cars.FindAsync(carId);
                if (car == null) return false;

                // Check if car has active bookings
                var hasActiveBookings = await _context.Bookings
                    .AnyAsync(b => b.CarID == carId && b.BookingStatus == BookingStatus.Confirmed);

                if (hasActiveBookings) return false;

                _context.Cars.Remove(car);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ToggleCarAvailabilityAsync(int carId)
        {
            try
            {
                var car = await _context.Cars.FindAsync(carId);
                if (car == null) return false;

                car.IsAvailable = !car.IsAvailable;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Booking management methods
        public async Task<IEnumerable<BookingViewModel>> GetAllBookingsAsync()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Car)
                .Include(b => b.Customer)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return bookings.Select(MapToBookingViewModel).ToList();
        }

        public async Task<bool> UpdateBookingStatusAsync(int bookingId, string status)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(bookingId);
                if (booking == null)
                    return false;

                if (Enum.TryParse<BookingStatus>(status, out var bookingStatus))
                {
                    booking.BookingStatus = bookingStatus;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ProcessRentAsync(int bookingId, int odometerStartReading, string paymentMethod, decimal partialPayment)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(bookingId);
                if (booking == null)
                    return false;

                booking.BookingStatus = BookingStatus.Rented;
                booking.OdometerStartReading = odometerStartReading;
                booking.AdvancePayment = partialPayment;
                booking.RemainingAmount = booking.TotalCost - partialPayment;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ProcessReturnAsync(int bookingId, DateTime returnDate, int odometerEndReading, string paymentStatus)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(bookingId);
                if (booking == null)
                    return false;

                booking.BookingStatus = BookingStatus.Completed;
                booking.ReturnDate = returnDate;
                booking.OdometerEndReading = odometerEndReading;
                
                if (booking.OdometerStartReading.HasValue)
                {
                    booking.DistanceTraveled = odometerEndReading - booking.OdometerStartReading.Value;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Admin password reset
        public async Task<bool> ResetAdminPasswordAsync(string adminEmail, string newPassword)
        {
            try
            {
                var admin = await _userManager.FindByEmailAsync(adminEmail);
                if (admin == null) return false;

                var token = await _userManager.GeneratePasswordResetTokenAsync(admin);
                var result = await _userManager.ResetPasswordAsync(admin, token, newPassword);
                return result.Succeeded;
            }
            catch
            {
                return false;
            }
        }

        // Helper methods
        private UserDto MapToUserDto(ApplicationUser user)
        {
            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString(),
                LicenseNumber = user.LicenseNumber,
                CompanyName = user.CompanyName,
                Address = user.Address,
                ImageUrl = user.ImageUrl,
                VerificationStatus = user.VerificationStatus,
                JoinDate = user.JoinDate
            };
        }

        private BookingDto MapToBookingDto(Booking booking)
        {
            return new BookingDto
            {
                BookingID = booking.BookingID, // Fix for CS1061: Use BookingID instead of Id
                CarID = booking.CarID,
                CustomerID = booking.CustomerID,
                PickupDate = booking.PickupDate,
                ReturnDate = booking.ReturnDate,
                BookingDate = booking.BookingDate,
                TotalCost = booking.TotalCost,
                BookingStatus = booking.BookingStatus,
                PickupLocation = booking.PickupLocation,
                ReturnLocation = booking.ReturnLocation,
                SpecialRequirements = booking.SpecialRequirements,
                StripeSessionId = booking.StripeSessionId,
                CarName = booking.Car?.CarName,
                UserName = booking.Customer?.FullName
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
                PickupLocation = booking.PickupLocation,
                ReturnLocation = booking.ReturnLocation,
                SpecialRequirements = booking.SpecialRequirements,
                WithDriver = booking.WithDriver,
                TotalAmount = booking.TotalCost,
                TotalCost = booking.TotalCost,
                Status = booking.BookingStatus.ToString(),
                BookingStatus = booking.BookingStatus,
                VehicleName = booking.Car?.CarName,
                CustomerName = booking.Customer?.FullName,
            };
        }

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

        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            try
            {
                return await _context.Users
                    .OrderByDescending(u => u.JoinDate)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<ApplicationUser>();
            }
        }

        public async Task<bool> UpdateUserAsync(ApplicationUser user)
        {
            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return false;

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ClearAllCarsAsync()
        {
            try
            {
                var allCars = await _context.Cars.ToListAsync();
                _context.Cars.RemoveRange(allCars);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
