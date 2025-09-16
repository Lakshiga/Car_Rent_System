using Car_Rent_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Car_Rent_System.Data;
using Newtonsoft.Json;

namespace Car_Rent_System.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string TraccarBaseUrl = "http://localhost:8082/api/";

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Check if user is logged in and has admin/staff role
            var userRole = HttpContext.Session.GetString("Role");
            if (!string.IsNullOrEmpty(userRole) && 
                (userRole == "Admin" || userRole == "SubAdmin" || userRole == "Staff"))
            {
                // Redirect admin users to their dashboard
                return RedirectToAction("Dashboard", "Admin");
            }

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

        // Traccar Integration Action
        [HttpGet]
        public async Task<IActionResult> VehicleLocation(long deviceId)
        {
            using var client = new HttpClient { BaseAddress = new Uri(TraccarBaseUrl) };

            // Create session (login)
            var loginData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("email", "admin"),
                new KeyValuePair<string, string>("password", "admin")
            });

            var loginRes = await client.PostAsync("session", loginData);
            if (!loginRes.IsSuccessStatusCode) return StatusCode((int)loginRes.StatusCode);

            // Fetch latest position
            var posRes = await client.GetAsync($"positions?deviceId={deviceId}&limit=1");
            if (!posRes.IsSuccessStatusCode) return StatusCode((int)posRes.StatusCode);

            var json = await posRes.Content.ReadAsStringAsync();
            var positions = JsonConvert.DeserializeObject<List<Position>>(json);
            return Json(positions.FirstOrDefault());
        }

        public IActionResult About()
        {
            // Check if user is logged in and has admin/staff role
            var userRole = HttpContext.Session.GetString("Role");
            if (!string.IsNullOrEmpty(userRole) && 
                (userRole == "Admin" || userRole == "SubAdmin" || userRole == "Staff"))
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            return View();
        }

        public IActionResult Contact()
        {
            // Check if user is logged in and has admin/staff role
            var userRole = HttpContext.Session.GetString("Role");
            if (!string.IsNullOrEmpty(userRole) && 
                (userRole == "Admin" || userRole == "SubAdmin" || userRole == "Staff"))
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            return View();
        }

        public IActionResult Privacy()
        {
            // Check if user is logged in and has admin/staff role
            var userRole = HttpContext.Session.GetString("Role");
            if (!string.IsNullOrEmpty(userRole) && 
                (userRole == "Admin" || userRole == "SubAdmin" || userRole == "Staff"))
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
