using Car_Rent_System.Models; 
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;



namespace Car_Rent_System.Controllers
{
    public class AdminController : Controller
    {
        private readonly CynexBlazerContext _context;

        private readonly Cloudinary _cloudinary;

        public AdminController(CynexBlazerContext context, Cloudinary cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        public async Task<IActionResult> Dashboard()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.TotalCars = await _context.Cars.CountAsync();
            ViewBag.AvailableCars = await _context.Cars.CountAsync(c => c.IsAvailable);
            ViewBag.TotalBookings = await _context.Bookings.CountAsync();
            ViewBag.TotalCustomers = await _context.Users.CountAsync(u => u.Role == "Customer");
            ViewBag.TotalRevenue = await _context.Bookings.SumAsync(b => b.TotalCost);

            var recentBookings = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Car)
                .OrderByDescending(b => b.BookingDate)
                .Take(5)
                .ToListAsync();

            return View(recentBookings);
        }

        public async Task<IActionResult> ManageCars()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var cars = await _context.Cars.OrderBy(c => c.CarName).ToListAsync();
            return View(cars);
        }

        [HttpGet]
        public IActionResult AddCar()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCar(Car car, IFormFile imageFile)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                // ✅ Upload image to Cloudinary
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(imageFile.FileName, imageFile.OpenReadStream()),
                        UseFilename = true,
                        UniqueFilename = false,
                        Overwrite = true
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    car.ImageUrl = uploadResult.SecureUrl.ToString(); // ✅ Save cloud URL to DB
                }

                car.DateAdded = DateTime.Now;
                _context.Cars.Add(car);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Car added successfully!";
                return RedirectToAction("ManageCars");
            }

            return View(car);
        }


        [HttpGet]
        public async Task<IActionResult> EditCar(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        [HttpPost]
        public async Task<IActionResult> EditCar(Car car)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                _context.Cars.Update(car);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Car updated successfully!";
                return RedirectToAction("ManageCars");
            }

            return View(car);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCar(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var car = await _context.Cars.FindAsync(id);
            if (car != null)
            {
                var hasActiveBookings = await _context.Bookings
                    .AnyAsync(b => b.CarID == id && b.BookingStatus == "Confirmed");

                if (hasActiveBookings)
                {
                    TempData["Error"] = "Cannot delete car with active bookings.";
                }
                else
                {
                    _context.Cars.Remove(car);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Car deleted successfully!";
                }
            }

            return RedirectToAction("ManageCars");
        }

        public async Task<IActionResult> ViewBookings()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var bookings = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Car)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return View(bookings);
        }

        public async Task<IActionResult> ManageUsers()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var users = await _context.Users
                .Where(u => u.Role == "Customer")
                .OrderBy(u => u.Username)
                .ToListAsync();

            return View(users);
        }
    }
}
