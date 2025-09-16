using Microsoft.AspNetCore.Mvc;
using Car_Rent_System.Models;
using Car_Rent_System.Services;
using Car_Rent_System.Interfaces;
using Car_Rent_System.ViewModels.Booking;

namespace Car_Rent_System.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IStripeService _stripeService;
        private readonly ICustomerService _customerService;

        public PaymentController(IStripeService stripeService, ICustomerService customerService)
        {
            _stripeService = stripeService;
            _customerService = customerService;
        }

        [HttpGet]
        public IActionResult PaymentSuccess(int bookingId)
        {
            ViewBag.BookingId = bookingId;
            ViewBag.Message = "Booking successfully confirmed. Please wait for admin approval.";
            return View();
        }

        [HttpGet]
        public IActionResult PaymentFailed()
        {
            ViewBag.Message = "Payment failed. Please try again.";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(BookingViewModel bookingViewModel, IFormFile licenseFrontFile, IFormFile licenseBackFile)
        {
            try
            {
                // Handle file uploads
                string licenseFrontUrl = "";
                string licenseBackUrl = "";

                if (licenseFrontFile != null && licenseFrontFile.Length > 0)
                {
                    licenseFrontUrl = await UploadFileAsync(licenseFrontFile);
                }

                if (licenseBackFile != null && licenseBackFile.Length > 0)
                {
                    licenseBackUrl = await UploadFileAsync(licenseBackFile);
                }

                // Update booking with file URLs
                bookingViewModel.LicenseImageFrontUrl = licenseFrontUrl;
                bookingViewModel.LicenseImageBackUrl = licenseBackUrl;

                // Create booking
                var customerId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(customerId))
                {
                    TempData["Error"] = "Please log in to make a booking.";
                    return RedirectToAction("Login", "Account");
                }

                var success = await _customerService.CreateBookingAsync(bookingViewModel, customerId);
                
                if (success)
                {
                    return RedirectToAction("PaymentSuccess", new { bookingId = bookingViewModel.Id });
                }
                else
                {
                    return RedirectToAction("PaymentFailed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ProcessPayment error: {ex.Message}");
                TempData["Error"] = "An error occurred while processing your booking.";
                return RedirectToAction("PaymentFailed");
            }
        }

        private async Task<string> UploadFileAsync(IFormFile file)
        {
            // For now, we'll use a simple file upload to wwwroot/uploads
            // In production, you should use a cloud service like Cloudinary, AWS S3, etc.
            
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{fileName}";
        }
    }
}
