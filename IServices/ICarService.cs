using Car_Rent_System.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Car_Rent_System.Interfaces
{
    public interface ICarService
    {
        Task<int> GetTotalCarsAsync();
        Task<int> GetAvailableCarsCountAsync();
        Task<CarDto> GetCarByIdAsync(int id);
        Task<IEnumerable<CarDto>> GetAllCarsAsync();
        Task CreateCarAsync(CarDto carDto);
        Task UpdateCarAsync(CarDto carDto);
        Task<bool> DeleteCarAsync(int id);
        Task<bool> ToggleCarAvailabilityAsync(int id);

        // ✅ Search and Filter Methods
        Task<IEnumerable<CarDto>> SearchCarsAsync(string searchTerm, string carType, string fuelType, decimal? maxPrice);
        Task<IEnumerable<string>> GetDistinctCarTypesAsync();
        Task<IEnumerable<string>> GetDistinctFuelTypesAsync();

        // ✅ Availability Update
        Task UpdateCarAvailabilityAsync(int carId, bool isAvailable);
    }
}