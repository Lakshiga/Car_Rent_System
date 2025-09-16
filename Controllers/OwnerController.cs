using Car_Rent_System.Data;
using Car_Rent_System.Enums;
using Car_Rent_System.Models;
using Car_Rent_System.ViewModels.Car;
using Car_Rent_System.ViewModels.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car_Rent_System.Controllers
{
    [Authorize(Roles = "VehicleOwner")]
    public class OwnerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OwnerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Owner/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var cars = await _context.Cars
                .Where(c => c.ApplicationUserId == user.Id)
                .ToListAsync();

            var activeRentals = await _context.Bookings
                .CountAsync(b => b.Car.ApplicationUserId == user.Id &&
                                 b.BookingStatus == BookingStatus.Confirmed);

            var monthlyEarnings = await _context.Bookings
                .Where(b => b.Car.ApplicationUserId == user.Id &&
                            b.BookingStatus == BookingStatus.Confirmed &&
                            b.BookingDate >= DateTime.Now.AddMonths(-1))
                .SumAsync(b => b.TotalCost);

            var dashboardModel = new DashboardViewModel
            {
                BusinessName = user.CompanyName ?? "My Fleet",
                TotalVehicles = cars.Count,
                ActiveRentals = activeRentals,
                MonthlyEarnings = monthlyEarnings,
                TotalDrivers = 0, // You can implement driver logic later if needed
                Cars = cars.Select(c => new CarSummaryViewModel
                {
                    Id = c.Id,
                    DisplayName = $"{c.Year} {c.Make} {c.CarModel}",
                    DailyRate = c.DailyRate,
                    IsAvailable = c.IsAvailable,
                    ImageUrl = c.ImageUrl
                }).ToList()
            };

            return View(dashboardModel);
        }

        // GET: Owner/ProfileSetup
        public async Task<IActionResult> ProfileSetup()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var model = new ProfileSetupViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                CompanyName = user.CompanyName,
                LicenseNumber = user.LicenseNumber,
                ImageUrl = user.ImageUrl
            };

            return View(model);
        }

        // POST: Owner/ProfileSetup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProfileSetup(ProfileSetupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.CompanyName = model.CompanyName;
            user.LicenseNumber = model.LicenseNumber;

            if (model.ImageFile != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                user.ImageUrl = "/images/" + fileName;
            }

            await _userManager.UpdateAsync(user);
            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Dashboard));
        }

        // Placeholder actions (you can implement later)
        public IActionResult Fleet() => View();
        public IActionResult Reports() => View();
        public IActionResult Team() => View();
    }
}