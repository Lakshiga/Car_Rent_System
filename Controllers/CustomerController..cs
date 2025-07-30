using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Car_Rent_System.Models;

namespace Car_Rent_System.Controllers
{
    public class CustomerController : Controller
    {
        private readonly CynexBlazerContext _context;

        public CustomerController(CynexBlazerContext context)
        {
            _context = context;
        }

        private bool IsCustomer()
        {
            return HttpContext.Session.GetString("Role") == "Customer";
        }

        private int? GetCustomerId()
        {
            return HttpContext.Session.GetInt32("UserID");
        }

        public async Task<IActionResult> Dashboard()
        {
            if (!IsCustomer())
                return RedirectToAction("Login", "Account");

            var customerId = GetCustomerId();

            ViewBag.TotalBookings = await _context.Bookings.CountAsync(b => b.CustomerID == customerId);
            ViewBag.TotalSpent = await _context.Bookings
                .Where(b => b.CustomerID == customerId)
                .SumAsync(b => b.TotalCost);

            var recentBookings = await _context.Bookings
                .Include(b => b.Car)
                .Where(b => b.CustomerID == customerId)
                .OrderByDescending(b => b.BookingDate)
                .Take(5)
                .ToListAsync();

            return View(recentBookings);
        }

        public async Task<IActionResult> BrowseCars(string searchTerm, string carType, string fuelType, decimal? maxPrice)
        {
            var query = _context.Cars.Where(c => c.IsAvailable);

            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(c => c.CarName.Contains(searchTerm) || c.CarModel.Contains(searchTerm));

            if (!string.IsNullOrEmpty(carType))
                query = query.Where(c => c.CarType == carType);

            if (!string.IsNullOrEmpty(fuelType))
                query = query.Where(c => c.FuelType == fuelType);

            if (maxPrice.HasValue)
                query = query.Where(c => c.DailyRate <= maxPrice.Value);

            var cars = await query.OrderBy(c => c.DailyRate).ToListAsync();

            ViewBag.CarTypes = await _context.Cars.Select(c => c.CarType).Distinct().ToListAsync();
            ViewBag.FuelTypes = await _context.Cars.Select(c => c.FuelType).Distinct().ToListAsync();

            return View(cars);
        }

        [HttpGet]
        public async Task<IActionResult> BookCar(int id)
        {
            if (!IsCustomer())
                return RedirectToAction("Login", "Account");

            var car = await _context.Cars.FindAsync(id);
            if (car == null || !car.IsAvailable)
            {
                TempData["Error"] = "Car is not available.";
                return RedirectToAction("BrowseCars");
            }

            ViewBag.Car = car;
            return View(new Booking { CarID = id });
        }

        [HttpPost]
        public async Task<IActionResult> BookCar(Booking booking)
        {
            if (!IsCustomer())
                return RedirectToAction("Login", "Account");

            var customerId = GetCustomerId();
            if (customerId == null)
                return RedirectToAction("Login", "Account");

            var car = await _context.Cars.FindAsync(booking.CarID);

            if (booking.PickupDate < DateTime.Today)
                ModelState.AddModelError("PickupDate", "Pickup date cannot be in the past.");

            if (booking.ReturnDate <= booking.PickupDate)
                ModelState.AddModelError("ReturnDate", "Return date must be after pickup date.");

            if (car == null || !car.IsAvailable)
                ModelState.AddModelError("", "Car is not available for booking.");

            if (ModelState.IsValid)
            {
                // Check for conflicting bookings
                var hasConflict = await _context.Bookings.AnyAsync(b =>
                    b.CarID == booking.CarID &&
                    b.BookingStatus == "Confirmed" &&
                    (
                        (booking.PickupDate >= b.PickupDate && booking.PickupDate <= b.ReturnDate) ||
                        (booking.ReturnDate >= b.PickupDate && booking.ReturnDate <= b.ReturnDate) ||
                        (booking.PickupDate <= b.PickupDate && booking.ReturnDate >= b.ReturnDate)
                    ));

                if (hasConflict)
                {
                    ModelState.AddModelError("", "Car is already booked for the selected dates.");
                    ViewBag.Car = car;
                    return View(booking);
                }

                booking.CustomerID = customerId.Value;
                booking.BookingDate = DateTime.Now;
                booking.BookingStatus = "Confirmed";

                var totalDays = (booking.ReturnDate - booking.PickupDate).Days + 1;
                booking.TotalCost = car.DailyRate * totalDays;

                _context.Bookings.Add(booking);
                car.IsAvailable = false;
                _context.Cars.Update(car);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Car booked successfully!";
                return RedirectToAction("BookingHistory");
            }

            ViewBag.Car = car;
            return View(booking);
        }

        public async Task<IActionResult> BookingHistory()
        {
            if (!IsCustomer())
                return RedirectToAction("Login", "Account");

            var customerId = GetCustomerId();

            var bookings = await _context.Bookings
                .Include(b => b.Car)
                .Where(b => b.CustomerID == customerId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return View(bookings);
        }

        [HttpPost]
        public async Task<IActionResult> CancelBooking(int id)
        {
            if (!IsCustomer())
                return RedirectToAction("Login", "Account");

            var customerId = GetCustomerId();
            var booking = await _context.Bookings
                .Include(b => b.Car)
                .FirstOrDefaultAsync(b => b.BookingID == id && b.CustomerID == customerId);

            if (booking != null && booking.BookingStatus == "Confirmed" && booking.PickupDate > DateTime.Today)
            {
                booking.BookingStatus = "Cancelled";
                booking.Car.IsAvailable = true;

                _context.Bookings.Update(booking);
                _context.Cars.Update(booking.Car);

                await _context.SaveChangesAsync();
                TempData["Success"] = "Booking cancelled successfully!";
            }
            else
            {
                TempData["Error"] = "Cannot cancel this booking.";
            }

            return RedirectToAction("BookingHistory");
        }
    }
}
