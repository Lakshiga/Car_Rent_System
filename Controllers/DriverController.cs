using Car_Rent_System.Models;
using Car_Rent_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Car_Rent_System.Controllers
{
    [Authorize(Roles = "Driver,Admin")]
    public class DriverController : Controller
    {
        private readonly IDriverService _driverService;
        private readonly UserManager<IdentityUser> _userManager;

        public DriverController(IDriverService driverService, UserManager<IdentityUser> userManager)
        {
            _driverService = driverService;
            _userManager = userManager;
        }

        // GET: Driver/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var driver = await _driverService.GetDriverByUserIdAsync(user.Id);
            if (driver == null)
            {
                // Redirect to profile setup if driver profile not created
                return RedirectToAction(nameof(ProfileSetup));
            }

            ViewData["DriverName"] = driver.FullName;
            return View(driver);
        }

        // GET: Driver/ProfileSetup
        public IActionResult ProfileSetup()
        {
            return View();
        }

        // POST: Driver/ProfileSetup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProfileSetup(Driver model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            model.UserId = user.Id;
            model.CreatedAt = DateTime.UtcNow;

            var success = await _driverService.CreateDriverAsync(model);
            if (success)
            {
                TempData["Success"] = "Profile created successfully!";
                return RedirectToAction(nameof(Dashboard));
            }

            ModelState.AddModelError("", "Failed to create profile.");
            return View(model);
        }

        // GET: Driver/Trips
        public IActionResult Trips()
        {
            // Placeholder - implement later
            return View();
        }

        // GET: Driver/Vehicle
        public IActionResult Vehicle()
        {
            // Placeholder
            return View();
        }

        // GET: Driver/Schedule
        public IActionResult Schedule()
        {
            // Placeholder
            return View();
        }
    }
}