using Car_Rent_System.Models;
using Car_Rent_System.Data;
using Car_Rent_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Car_Rent_System.Services
{
    public class DriverService : IDriverService
    {
        private readonly ApplicationDbContext _context;

        public DriverService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Driver?> GetDriverByUserIdAsync(string userId)
        {
            return await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId);
        }

        public async Task<Driver?> GetDriverByIdAsync(int id)
        {
            return await _context.Drivers.FindAsync(id);
        }

        public async Task<List<Driver>> GetAllDriversAsync()
        {
            return await _context.Drivers
                .Include(d => d.User)
                .ToListAsync();
        }

        public async Task<bool> CreateDriverAsync(Driver driver)
        {
            if (driver == null) return false;

            await _context.Drivers.AddAsync(driver);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> UpdateDriverAsync(Driver driver)
        {
            if (driver == null) return false;

            _context.Drivers.Update(driver);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteDriverAsync(int id)
        {
            var driver = await _context.Drivers.FindAsync(id);
            if (driver == null) return false;

            _context.Drivers.Remove(driver);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
    }
}