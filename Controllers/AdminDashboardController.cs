using Microsoft.AspNetCore.Mvc;
using CarRentalSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.Security.Claims;

namespace CarRentalSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Cloudinary _cloudinary;

        public AdminController(ApplicationDbContext context, Cloudinary cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
        }

        public async Task<IActionResult> Dashboard()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalCars = await _context.Cars.CountAsync();
            var totalBookings = await _context.Bookings.CountAsync();
            var availableCars = await _context.Cars.CountAsync(c => c.IsAvailable);
            var recentBookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Car)
                .OrderByDescending(b => b.BookingDate)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalCars = totalCars;
            ViewBag.TotalBookings = totalBookings;
            ViewBag.AvailableCars = availableCars;
            ViewBag.RecentBookings = recentBookings;

            return View();
        }

        public async Task<IActionResult> ManageCars()
        {
            var cars = await _context.Cars.OrderBy(c => c.Make).ThenBy(c => c.Model).ToListAsync();
            return View(cars);
        }

        [HttpGet]
        public IActionResult AddCar()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCar(Car car, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Handle image upload
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(imageFile.FileName, imageFile.OpenReadStream()),
                            Folder = "cynex-blazer/cars",
                            PublicId = $"car_{DateTime.Now.Ticks}",
                            Transformation = new Transformation().Width(800).Height(600).Crop("fill").Quality("auto")
                        };

                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                        if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            car.ImageUrl = uploadResult.SecureUrl.ToString();
                        }
                        else
                        {
                            TempData["Error"] = "Failed to upload image. Please try again.";
                            return View(car);
                        }
                    }
                    else
                    {
                        // Use placeholder if no image uploaded
                        car.ImageUrl = $"/placeholder.svg?height=300&width=400";
                    }

                    _context.Cars.Add(car);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"{car.Make} {car.Model} has been added to the fleet successfully!";
                    return RedirectToAction("ManageCars");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "An error occurred while adding the car. Please try again.";
                    return View(car);
                }
            }

            return View(car);
        }

        [HttpGet]
        public async Task<IActionResult> EditCar(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                TempData["Error"] = "Car not found.";
                return RedirectToAction("ManageCars");
            }

            return View(car);
        }

        [HttpPost]
        public async Task<IActionResult> EditCar(Car car, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingCar = await _context.Cars.FindAsync(car.CarID);
                    if (existingCar == null)
                    {
                        TempData["Error"] = "Car not found.";
                        return RedirectToAction("ManageCars");
                    }

                    // Handle image upload
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(imageFile.FileName, imageFile.OpenReadStream()),
                            Folder = "cynex-blazer/cars",
                            PublicId = $"car_{car.CarID}_{DateTime.Now.Ticks}",
                            Transformation = new Transformation().Width(800).Height(600).Crop("fill").Quality("auto")
                        };

                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                        if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            car.ImageUrl = uploadResult.SecureUrl.ToString();
                        }
                        else
                        {
                            car.ImageUrl = existingCar.ImageUrl; // Keep existing image
                        }
                    }
                    else
                    {
                        car.ImageUrl = existingCar.ImageUrl; // Keep existing image
                    }

                    // Update properties
                    existingCar.Make = car.Make;
                    existingCar.Model = car.Model;
                    existingCar.Year = car.Year;
                    existingCar.Color = car.Color;
                    existingCar.PricePerDay = car.PricePerDay;
                    existingCar.IsAvailable = car.IsAvailable;
                    existingCar.ImageUrl = car.ImageUrl;
                    existingCar.Description = car.Description;

                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"{car.Make} {car.Model} has been updated successfully!";
                    return RedirectToAction("ManageCars");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "An error occurred while updating the car. Please try again.";
                    return View(car);
                }
            }

            return View(car);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCar(int id)
        {
            try
            {
                var car = await _context.Cars.FindAsync(id);
                if (car == null)
                {
                    TempData["Error"] = "Car not found.";
                    return RedirectToAction("ManageCars");
                }

                // Check if car has active bookings
                var hasActiveBookings = await _context.Bookings
                    .AnyAsync(b => b.CarID == id && b.Status == "Confirmed");

                if (hasActiveBookings)
                {
                    TempData["Error"] = "Cannot delete car with active bookings.";
                    return RedirectToAction("ManageCars");
                }

                _context.Cars.Remove(car);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"{car.Make} {car.Model} has been removed from the fleet.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while deleting the car.";
            }

            return RedirectToAction("ManageCars");
        }

        public async Task<IActionResult> ManageUsers()
        {
            var users = await _context.Users.OrderBy(u => u.Username).ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> ManageBookings()
        {
            var bookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Car)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return View(bookings);
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return null;
        }

        public async Task<IActionResult> profile
        {
            return View(profile) ;
        }
    }
}

//using Car_Rent_System.Models;
//using Car_Rent_System.ViewModels;
//using Microsoft.AspNetCore.Mvc;
//using Car_Rent_System.Ennum;

//namespace Car_Rent_System.Controllers
//{
//    public class AdminDashboardController : Controller
//    {
//        public IActionResult Sidebar()
//        {
//            var user = new User2
//            {
//                FullName = "Admin User",
//                Email = "admin@carrent.com",
//                ProfileImageUrl = "/images/profile-placeholder.png",
//                Role = Ennum.UserRole.SuperAdmin
//            };

//            return PartialView("Sidebar", user);
//        }

//        public IActionResult Topbar()
//        {
//            var user = new User2
//            {
//                FullName = "Admin User",
//                Email = "admin@carrent.com",
//                ProfileImageUrl = "/images/profile-placeholder.png",
//                Role = Ennum.UserRole.SuperAdmin
//            };

//            // Pass the user object to the view, assuming the view expects a User2 model
//            return View(user);
//        }

//        public IActionResult Index()
//        {
//            // Normally this would come from Service + Repo
//            var vm = new AdminDashboardViewModel
//            {
//                TotalRevenue = 124500,
//                ActiveRentals = 89,
//                TotalCustomers = 2847,
//                FleetUtilizationPercent = 78,
//                RecentRentals = new List<RentalActivityViewModel>
//                {
//                    new RentalActivityViewModel { CustomerName = "John Smith", CarDisplay = "Tesla Model 3", Status = "Active", AgoText = "2 hours ago" },
//                    new RentalActivityViewModel { CustomerName = "Sarah Johnson", CarDisplay = "BMW X5", Status = "Completed", AgoText = "4 hours ago" },
//                    new RentalActivityViewModel { CustomerName = "Mike Wilson", CarDisplay = "Audi A4", Status = "Pending", AgoText = "6 hours ago" }
//                },
//                FleetPerformance = new FleetPerformanceViewModel
//                {
//                    Available = 45,
//                    Rented = 18,
//                    Maintenance = 6
//                }
//            };

//            return View(vm);
//        }

//        private IActionResult View(User2 user)
//        {
//            throw new NotImplementedException();
//        }

//        public IActionResult Profile()
//        {
//            var profileVm = new ProfileViewModel
//            {
//                FullName = "Admin User",
//                Role = "SuperAdmin",
//                Email = "admin@carrent.com",
//                ProfileImageUrl = "/images/profile-placeholder.png"
//            };

//            return View(profileVm);
//        }

//        private IActionResult View(ProfileViewModel profileVm)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}

