using System.Threading.Tasks;
using Car_Rent_System.DTOs;
using Car_Rent_System.Enums;
using Car_Rent_System.Interfaces;
using Car_Rent_System.Models;
using Car_Rent_System.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Stripe; 
using Newtonsoft.Json;
using Stripe.BillingPortal;
using BookingViewModel = Car_Rent_System.ViewModels.Booking.BookingViewModel;
using CustomerBookingViewModel = Car_Rent_System.ViewModels.Customer.BookingViewModel;
using DashboardViewModel = Car_Rent_System.ViewModels.Customer.DashboardViewModel;

namespace Car_Rent_System.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly IStripeService _stripeService;

        public CustomerController(
            ICustomerService customerService,
            IStripeService stripeService)
        {
            _customerService = customerService;
            _stripeService = stripeService;
        }

        private async Task<bool> IsCustomer()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId)) return false;

                var user = await _customerService.GetCustomerProfileAsync(userId);
                return user != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ IsCustomer error: {ex.Message}");
                return false;
            }
        }

        private string GetCustomerId()
        {
            return HttpContext.Session.GetString("UserId") ?? string.Empty;
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                if (!await IsCustomer())
                    return RedirectToAction("Login", "Account");

                var customerId = GetCustomerId();
                if (string.IsNullOrEmpty(customerId))
                {
                    TempData["Error"] = "Customer session not found.";
                    return RedirectToAction("Login", "Account");
                }

                var dashboardData = await _customerService.GetCustomerDashboardAsync(customerId);
                return View(dashboardData.RecentBookings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Dashboard error: {ex.Message}");
                TempData["Error"] = "An error occurred while loading the dashboard.";
                return RedirectToAction("Login", "Account");
            }
        }

        public async Task<IActionResult> BrowseCars(string searchTerm, string carType, string fuelType, decimal? maxPrice)
        {
            try
            {
                if (!await IsCustomer())
                    return RedirectToAction("Login", "Account");

                var cars = await _customerService.SearchCarsAsync(searchTerm, carType, fuelType, maxPrice);
                ViewBag.CarTypes = await _customerService.GetDistinctCarTypesAsync();
                ViewBag.FuelTypes = await _customerService.GetDistinctFuelTypesAsync();

                return View(cars);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ BrowseCars error: {ex.Message}");
                TempData["Error"] = "An error occurred while browsing cars.";
                return RedirectToAction("Dashboard");
            }
        }

        [HttpGet]
        public async Task<IActionResult> BookCar(int id)
        {
            try
            {
                if (!await IsCustomer())
                    return RedirectToAction("Login", "Account");

                var car = await _customerService.GetCarByIdAsync(id);
                if (car == null || !car.IsAvailable)
                {
                    TempData["Error"] = "Car is not available for booking.";
                    return RedirectToAction("BrowseCars");
                }

                ViewBag.Car = car;
                return View(new BookingViewModel { VehicleId = id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ BookCar GET error: {ex.Message}");
                TempData["Error"] = "An error occurred while loading the booking page.";
                return RedirectToAction("BrowseCars");
            }
        }

        [HttpPost]
        public async Task<IActionResult> BookCar(BookingViewModel bookingViewModel, IFormFile licenseFrontFile, IFormFile licenseBackFile)
        {
            try
            {
                if (!await IsCustomer())
                    return RedirectToAction("Login", "Account");

                var customerId = GetCustomerId();
                if (string.IsNullOrEmpty(customerId))
                {
                    TempData["Error"] = "Customer session not found.";
                    return RedirectToAction("Login", "Account");
                }

                var car = await _customerService.GetCarByIdAsync(bookingViewModel.VehicleId);
                if (car == null || !car.IsAvailable)
                {
                    ModelState.AddModelError("", "Car is no longer available.");
                    ViewBag.Car = car;
                    return View(bookingViewModel);
                }

                // Validate dates
                if (bookingViewModel.PickupDate < DateTime.Today)
                    ModelState.AddModelError("PickupDate", "Pickup date cannot be in the past.");

                if (bookingViewModel.ReturnDate <= bookingViewModel.PickupDate)
                    ModelState.AddModelError("ReturnDate", "Return date must be after pickup date.");

                // Validate file uploads
                if (licenseFrontFile == null || licenseFrontFile.Length == 0)
                {
                    ModelState.AddModelError("", "License front image is required.");
                }

                if (licenseBackFile == null || licenseBackFile.Length == 0)
                {
                    ModelState.AddModelError("", "License back image is required.");
                }

                if (ModelState.IsValid)
                {
                    // Check for booking conflicts
                    var hasConflict = await _customerService.HasBookingConflictAsync(bookingViewModel.VehicleId, bookingViewModel.PickupDate, bookingViewModel.ReturnDate);
                    if (hasConflict)
                    {
                        ModelState.AddModelError("", "Car is already booked for the selected dates.");
                        ViewBag.Car = car;
                        return View(bookingViewModel);
                    }

                    // Calculate total cost using the new method
                    var totalCost = await _customerService.CalculateBookingCostAsync(bookingViewModel);
                    if (totalCost <= 0)
                    {
                        ModelState.AddModelError("", "Unable to calculate booking cost. Please check your inputs.");
                        ViewBag.Car = car;
                        return View(bookingViewModel);
                    }

                    // Store booking data in TempData for payment processing
                    TempData["BookingViewModel"] = Newtonsoft.Json.JsonConvert.SerializeObject(bookingViewModel);
                    TempData["LicenseFrontFile"] = licenseFrontFile?.FileName;
                    TempData["LicenseBackFile"] = licenseBackFile?.FileName;
                    
                    // For now, redirect to payment success (in real implementation, this would be payment processing)
                    return RedirectToAction("PaymentSuccess", "Payment", new { bookingId = 1 });
                }

                ViewBag.Car = car;
                return View(bookingViewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ BookCar POST error: {ex.Message}");
                TempData["Error"] = "An error occurred while processing the booking.";
                return RedirectToAction("BrowseCars");
            }
        }


        public async Task<IActionResult> Checkout(string sessionId)
        {
            try
            {
                if (!await IsCustomer())
                    return RedirectToAction("Login", "Account");

                if (string.IsNullOrEmpty(sessionId))
                {
                    TempData["Error"] = "Invalid checkout session.";
                    return RedirectToAction("BrowseCars");
                }

                ViewBag.SessionId = sessionId;
                ViewBag.StripePublishableKey = "pk_test_51...";
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Checkout error: {ex.Message}");
                TempData["Error"] = "An error occurred while loading checkout.";
                return RedirectToAction("BrowseCars");
            }
        }
        [HttpPost]
        public async Task<IActionResult> Webhook()
        {
            try
            {
                var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

                // ✅ Now this correctly uses Stripe.EventUtility
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    "whsec_your_webhook_secret_here" // 👉 Replace with your actual webhook secret from Stripe Dashboard
                );

                // ✅ Now this correctly uses Stripe.Events
                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Session;
                    var bookingInfo = TempData["BookingInfo"]?.ToString();
                    var carId = TempData["CarID"]?.ToString();

                    if (!string.IsNullOrEmpty(bookingInfo) && !string.IsNullOrEmpty(carId))
                    {
                        var bookingDto = JsonConvert.DeserializeObject<BookingDto>(bookingInfo);
                        if (bookingDto == null) return BadRequest("Invalid booking data");

                        // ✅ Map DTO → ViewModel
                        var bookingViewModel = new BookingViewModel
                        {
                            VehicleId = bookingDto.CarID,
                            PickupDate = bookingDto.PickupDate,
                            ReturnDate = bookingDto.ReturnDate,
                            TotalAmount = bookingDto.TotalCost,
                            Status = bookingDto.BookingStatus.ToString(),
                            PickupLocation = bookingDto.PickupLocation,
                            ReturnLocation = bookingDto.ReturnLocation,
                            SpecialRequirements = bookingDto.SpecialRequirements,
                            WithDriver = bookingDto.WithDriver
                        };

                        // ✅ Get userId
                        var userId = GetCustomerId();

                        // ✅ Call service with userId
                        var success = await _customerService.CreateBookingAsync(bookingViewModel, userId);

                        if (success)
                        {
                            TempData["Success"] = "Payment successful! Your booking is confirmed.";
                        }
                        else
                        {
                            return BadRequest("Failed to create booking.");
                        }
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Webhook error: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        public async Task<IActionResult> BookingHistory()
        {
            try
            {
                if (!await IsCustomer())
                    return RedirectToAction("Login", "Account");

                var customerId = GetCustomerId();
                if (string.IsNullOrEmpty(customerId))
                {
                    TempData["Error"] = "Customer session not found.";
                    return RedirectToAction("Login", "Account");
                }

                var bookings = await _customerService.GetCustomerAllBookingsAsync(customerId);
                return View(bookings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ BookingHistory error: {ex.Message}");
                TempData["Error"] = "An error occurred while loading booking history.";
                return RedirectToAction("Dashboard");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancelBooking(int id)
        {
            try
            {
                if (!await IsCustomer())
                    return RedirectToAction("Login", "Account");

                var customerId = GetCustomerId();
                if (string.IsNullOrEmpty(customerId))
                {
                    TempData["Error"] = "Customer session not found.";
                    return RedirectToAction("Login", "Account");
                }

                var success = await _customerService.CancelBookingAsync(id, customerId);

                if (success)
                {
                    TempData["Success"] = "Booking cancelled successfully!";
                }
                else
                {
                    TempData["Error"] = "Cannot cancel this booking. It may be too late or already cancelled.";
                }

                return RedirectToAction("BookingHistory");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ CancelBooking error: {ex.Message}");
                TempData["Error"] = "An error occurred while cancelling the booking.";
                return RedirectToAction("BookingHistory");
            }
        }

        public async Task<IActionResult> RentalHistory()
        {
            try
            {
                if (!await IsCustomer())
                    return RedirectToAction("Login", "Account");

                var customerId = GetCustomerId();
                if (string.IsNullOrEmpty(customerId))
                {
                    TempData["Error"] = "Customer session not found.";
                    return RedirectToAction("Login", "Account");
                }

                var bookings = await _customerService.GetCustomerRecentBookingsAsync(customerId, 50);
                return View(bookings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ RentalHistory error: {ex.Message}");
                TempData["Error"] = "An error occurred while loading rental history.";
                return RedirectToAction("Dashboard");
            }
        }

        public async Task<IActionResult> ReturnHistory()
        {
            try
            {
                if (!await IsCustomer())
                    return RedirectToAction("Login", "Account");

                var customerId = GetCustomerId();
                if (string.IsNullOrEmpty(customerId))
                {
                    TempData["Error"] = "Customer session not found.";
                    return RedirectToAction("Login", "Account");
                }

                var bookings = await _customerService.GetCustomerRecentBookingsAsync(customerId, 50);
                return View(bookings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ReturnHistory error: {ex.Message}");
                TempData["Error"] = "An error occurred while loading return history.";
                return RedirectToAction("Dashboard");
            }
        }

        public async Task<IActionResult> PaymentHistory()
        {
            try
            {
                if (!await IsCustomer())
                    return RedirectToAction("Login", "Account");

                var customerId = GetCustomerId();
                if (string.IsNullOrEmpty(customerId))
                {
                    TempData["Error"] = "Customer session not found.";
                    return RedirectToAction("Login", "Account");
                }

                var bookings = await _customerService.GetCustomerRecentBookingsAsync(customerId, 50);
                return View(bookings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ PaymentHistory error: {ex.Message}");
                TempData["Error"] = "An error occurred while loading payment history.";
                return RedirectToAction("Dashboard");
            }
        }
    }
}