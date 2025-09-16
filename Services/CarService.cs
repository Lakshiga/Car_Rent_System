using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Car_Rent_System.Data;
using Car_Rent_System.DTOs;
using Car_Rent_System.Enums;
using Car_Rent_System.Interfaces;
using Car_Rent_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Car_Rent_System.Services
{
    public class CarService : ICarService
    {
        private readonly ApplicationDbContext _context;

        public CarService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetTotalCarsAsync()
        {
            return await _context.Cars.CountAsync();
        }

        public async Task<int> GetAvailableCarsCountAsync()
        {
            return await _context.Cars.CountAsync(c => c.IsAvailable);
        }

        public async Task<CarDto> GetCarByIdAsync(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            return car == null ? null : MapToDto(car);
        }

        public async Task<IEnumerable<CarDto>> GetAllCarsAsync()
        {
            var cars = await _context.Cars.ToListAsync();
            return cars.Select(MapToDto);
        }

        public async Task CreateCarAsync(CarDto carDto)
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
                ImageUrl = carDto.ImageUrl,
                IsAvailable = carDto.IsAvailable,
                DateAdded = DateTime.UtcNow
            };

            await _context.Cars.AddAsync(car);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCarAsync(CarDto carDto)
        {
            var car = await _context.Cars.FindAsync(carDto.CarID);
            if (car == null)
                throw new KeyNotFoundException($"Car with ID {carDto.CarID} not found.");

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

            _context.Cars.Update(car);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteCarAsync(int id)
        {
            var hasActiveBookings = await _context.Bookings.AnyAsync(b => b.CarID == id && b.BookingStatus == BookingStatus.Confirmed);
            if (hasActiveBookings) return false;

            var car = await _context.Cars.FindAsync(id);
            if (car == null) return false;

            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleCarAvailabilityAsync(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null) return false;

            car.IsAvailable = !car.IsAvailable;
            _context.Cars.Update(car);
            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ IMPLEMENTED: SearchCarsAsync
        public async Task<IEnumerable<CarDto>> SearchCarsAsync(string searchTerm, string carType, string fuelType, decimal? maxPrice)
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

            var cars = await query.ToListAsync();
            return cars.Select(MapToDto);
        }

        // ✅ IMPLEMENTED: GetDistinctCarTypesAsync
        public async Task<IEnumerable<string>> GetDistinctCarTypesAsync()
        {
            return await _context.Cars
                .Where(c => c.CarType != null)
                .Select(c => c.CarType)
                .Distinct()
                .ToListAsync();
        }

        // ✅ IMPLEMENTED: GetDistinctFuelTypesAsync
        public async Task<IEnumerable<string>> GetDistinctFuelTypesAsync()
        {
            return await _context.Cars
                .Where(c => c.FuelType != null)
                .Select(c => c.FuelType)
                .Distinct()
                .ToListAsync();
        }

        // ✅ IMPLEMENTED: UpdateCarAvailabilityAsync
        public async Task UpdateCarAvailabilityAsync(int carId, bool isAvailable)
        {
            var car = await _context.Cars.FindAsync(carId);
            if (car != null)
            {
                car.IsAvailable = isAvailable;
                _context.Cars.Update(car);
                await _context.SaveChangesAsync();
            }
        }

        private CarDto MapToDto(Car car)
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
    }
}