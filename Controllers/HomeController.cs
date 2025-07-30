using Microsoft.AspNetCore.Mvc;
using Car_Rent_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Car_Rent_System.Controllers
{
    public class HomeController : Controller
    {
        private readonly CynexBlazerContext _context;

        public HomeController(CynexBlazerContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var featuredCars = await _context.Cars
                .Where(c => c.IsAvailable)
                .OrderBy(c => c.DailyRate)
                .Take(6)
                .ToListAsync();

            ViewBag.TotalCars = await _context.Cars.CountAsync();
            ViewBag.AvailableCars = await _context.Cars.CountAsync(c => c.IsAvailable);
            ViewBag.TotalBookings = await _context.Bookings.CountAsync();

            return View(featuredCars);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
