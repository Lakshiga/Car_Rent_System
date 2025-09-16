using Car_Rent_System.Models;
using Car_Rent_System.Data;
using Car_Rent_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Car_Rent_System.Services
{
    public class OwnerService : IOwnerService
    {
        private readonly ApplicationDbContext _context;

        public OwnerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Owner?> GetOwnerByUserIdAsync(string userId)
        {
            return await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);
        }

        public async Task<Owner?> GetOwnerByIdAsync(int id)
        {
            return await _context.Owners.FindAsync(id);
        }

        public async Task<List<Owner>> GetAllOwnersAsync()
        {
            return await _context.Owners
                .Include(o => o.User)
                .ToListAsync();
        }

        public async Task<bool> CreateOwnerAsync(Owner owner)
        {
            if (owner == null) return false;

            await _context.Owners.AddAsync(owner);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> UpdateOwnerAsync(Owner owner)
        {
            if (owner == null) return false;

            _context.Owners.Update(owner);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteOwnerAsync(int id)
        {
            var owner = await _context.Owners.FindAsync(id);
            if (owner == null) return false;

            _context.Owners.Remove(owner);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
    }
}