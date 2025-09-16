using System.Threading.Tasks;
using Car_Rent_System.DTOs;
using Car_Rent_System.Enums;
using Car_Rent_System.Interfaces;
using Car_Rent_System.Models;
using Car_Rent_System.Services;
using Car_Rent_System.ViewModels.Admin;
using Car_Rent_System.ViewModels.Booking;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car_Rent_System.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // Helper method to check admin authorization using session
        private bool IsAdminAuthorized()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("Role");
            
            return !string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(userRole) && 
                   (userRole == "Admin" || userRole == "SubAdmin" || userRole == "Staff");
        }

        // Helper method to redirect if not authorized
        private IActionResult RedirectIfNotAuthorized()
        {
            TempData["Error"] = "You are not authorized to access this area.";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public async Task<IActionResult> ResetAdminPassword()
        {
            try
            {
                var (success, message) = await _adminService.ResetAdminPasswordAsync("lakshiga20021216@gmail.com", "YourNewStrongPassword123!");
                
                if (success)
                {
                    TempData["Success"] = message;
                }
                else
                {
                    TempData["Error"] = message;
                }
                
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ResetAdminPassword error: {ex.Message}");
                TempData["Error"] = "An error occurred while resetting password.";
                return RedirectToAction("Login");
            }
        }

        // Admin Dashboard - Using session-based authentication
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                if (!IsAdminAuthorized())
                    return RedirectIfNotAuthorized();

                var userId = HttpContext.Session.GetString("UserId");
                
                // Use service layer for dashboard data
                ViewBag.DashboardTitle = await _adminService.GetDashboardTitleAsync(userId);
                ViewBag.FullName = HttpContext.Session.GetString("FullName");

                var (totalCars, availableCars, totalBookings, totalRevenue) = await _adminService.GetDashboardStatsAsync();
                ViewBag.TotalCars = totalCars;
                ViewBag.AvailableCars = availableCars;
                ViewBag.TotalBookings = totalBookings;
                ViewBag.TotalRevenue = totalRevenue;

                var recentBookings = await _adminService.GetRecentBookingsAsync(5);
                return View(recentBookings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Dashboard error: {ex.Message}");
                TempData["Error"] = "An error occurred while loading the dashboard.";
                return RedirectToAction("Login", "Account");
            }
        }

        // Pending User Approvals
        public async Task<IActionResult> PendingUsers()
        {
            try
            {
                if (!IsAdminAuthorized())
                    return RedirectIfNotAuthorized();

                var users = await _adminService.GetPendingUsersAsync();
                return View(users);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ PendingUsers error: {ex.Message}");
                TempData["Error"] = "An error occurred while loading pending users.";
                return RedirectToAction("Dashboard");
            }
        }

        // Approve User
        [HttpPost]
        public async Task<IActionResult> ApproveUser(string id)
        {
            try
            {
                if (!IsAdminAuthorized())
                    return RedirectIfNotAuthorized();

                var (success, message) = await _adminService.ApproveUserAsync(id);
                
                if (success)
                {
                    TempData["Success"] = message;
                }
                else
                {
                    TempData["Error"] = message;
                }

                return RedirectToAction("PendingUsers");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ApproveUser error: {ex.Message}");
                TempData["Error"] = "An error occurred while approving the user.";
                return RedirectToAction("PendingUsers");
            }
        }

        // Reject User
        [HttpPost]
        public async Task<IActionResult> RejectUser(string id)
        {
            try
            {
                if (!IsAdminAuthorized())
                    return RedirectIfNotAuthorized();

                var (success, message) = await _adminService.RejectUserAsync(id);
                
                if (success)
                {
                    TempData["Success"] = message;
                }
                else
                {
                    TempData["Error"] = message;
                }

                return RedirectToAction("PendingUsers");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ RejectUser error: {ex.Message}");
                TempData["Error"] = "An error occurred while rejecting the user.";
                return RedirectToAction("PendingUsers");
            }
        }

        // Manage Users
        public async Task<IActionResult> ManageUsers()
        {
            try
            {
                if (!IsAdminAuthorized())
                    return RedirectIfNotAuthorized();

                var users = await _adminService.GetAllApprovedUsersAsync();
                return View(users);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ManageUsers error: {ex.Message}");
                TempData["Error"] = "An error occurred while loading users.";
                return RedirectToAction("Dashboard");
            }
        }

        // Manage Cars
        public async Task<IActionResult> ManageCars()
        {
            try
            {
                if (!IsAdminAuthorized())
                    return RedirectIfNotAuthorized();

                var cars = await _adminService.GetAllCarsAsync();
                return View(cars);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ManageCars error: {ex.Message}");
                TempData["Error"] = "An error occurred while loading the fleet.";
                return RedirectToAction("Dashboard");
            }
        }

        // View Bookings
        public async Task<IActionResult> ViewBookings()
        {
            try
            {
                if (!IsAdminAuthorized())
                    return RedirectIfNotAuthorized();

                var bookings = await _adminService.GetAllBookingsAsync();
                return View(bookings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ViewBookings error: {ex.Message}");
                TempData["Error"] = "An error occurred while loading bookings.";
                return RedirectToAction("Dashboard");
            }
        }


        // Rent Management
        public async Task<IActionResult> RentManagement()
        {
            try
            {
                if (!IsAdminAuthorized())
                    return RedirectIfNotAuthorized();

                // Get approved bookings that are ready for rent
                var bookings = await _adminService.GetAllBookingsAsync();
                var rentableBookings = bookings.Where(b => b.BookingStatus == BookingStatus.Approved).ToList();
                
                return View(rentableBookings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ RentManagement error: {ex.Message}");
                TempData["Error"] = "An error occurred while loading rent management.";
                return RedirectToAction("Dashboard");
            }
        }

        // Return Management
        public async Task<IActionResult> ReturnManagement()
        {
            try
            {
                if (!IsAdminAuthorized())
                    return RedirectIfNotAuthorized();

                // Get rented bookings that are ready for return
                var bookings = await _adminService.GetAllBookingsAsync();
                var returnableBookings = bookings.Where(b => b.BookingStatus == BookingStatus.Rented).ToList();
                
                return View(returnableBookings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ReturnManagement error: {ex.Message}");
                TempData["Error"] = "An error occurred while loading return management.";
                return RedirectToAction("Dashboard");
            }
        }

        // Update Booking Status
        [HttpPost]
        public async Task<IActionResult> UpdateBookingStatus([FromBody] UpdateBookingStatusRequest request)
        {
            try
            {
                if (!IsAdminAuthorized())
                    return Json(new { success = false, message = "Unauthorized" });

                var success = await _adminService.UpdateBookingStatusAsync(request.BookingId, request.Status);
                
                if (success)
                {
                    return Json(new { success = true, message = "Booking status updated successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update booking status" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ UpdateBookingStatus error: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while updating booking status" });
            }
        }

        // Process Rent
        [HttpPost]
        public async Task<IActionResult> ProcessRent([FromBody] ProcessRentRequest request)
        {
            try
            {
                if (!IsAdminAuthorized())
                    return Json(new { success = false, message = "Unauthorized" });

                var success = await _adminService.ProcessRentAsync(request.BookingId, request.OdometerStartReading, request.PaymentMethod, request.PartialPayment);
                
                if (success)
                {
                    return Json(new { success = true, message = "Rent processed successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to process rent" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ProcessRent error: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while processing rent" });
            }
        }

        // Process Return
        [HttpPost]
        public async Task<IActionResult> ProcessReturn([FromBody] ProcessReturnRequest request)
        {
            try
            {
                if (!IsAdminAuthorized())
                    return Json(new { success = false, message = "Unauthorized" });

                var success = await _adminService.ProcessReturnAsync(request.BookingId, request.ReturnDate, request.OdometerEndReading, request.PaymentStatus);
                
                if (success)
                {
                    return Json(new { success = true, message = "Return processed successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to process return" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ProcessReturn error: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while processing return" });
            }
        }

        // Add Car (GET)
        [HttpGet]
        public IActionResult AddCar()
        {
            try
            {
                if (!IsAdminAuthorized())
                    return RedirectIfNotAuthorized();

                return View(new Car());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ AddCar GET error: {ex.Message}");
                TempData["Error"] = "An error occurred while loading the add car page.";
                return RedirectToAction("Dashboard");
            }
        }

        // Add Car (POST)
        [HttpPost]
        public async Task<IActionResult> AddCar(Car car, IFormFile imageFile)
        {
            try
            {
                if (!IsAdminAuthorized())
                    return RedirectIfNotAuthorized();

                if (!ModelState.IsValid)
                {
                    return View(car);
                }

                var adminUserId = HttpContext.Session.GetString("UserId");
                
                // Convert Car to CarDto for service
                var carDto = new CarDto
                {
                    CarName = car.CarName,
                    CarModel = car.CarModel,
                    CarType = car.CarType,
                    DailyRate = car.DailyRate,
                    FuelType = car.FuelType,
                    Transmission = car.Transmission,
                    SeatingCapacity = car.SeatingCapacity,
                    Mileage = (decimal?)car.Mileage,
                    Description = car.Description,
                    IsAvailable = true
                };

                var (success, message) = await _adminService.CreateCarAsync(carDto, imageFile, adminUserId);
                
                if (success)
                {
                    TempData["Success"] = message;
                    return RedirectToAction("ManageCars");
                }
                else
                {
                    ModelState.AddModelError("", message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ AddCar POST error: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while adding the car.");
            }

            return View(car);
        }

        // Edit Car (GET)
        [HttpGet]
        public async Task<IActionResult> EditCar(int id)
        {
            try
            {
                if (!IsAdminAuthorized())
                    return RedirectIfNotAuthorized();

                var car = await _adminService.GetCarByIdAsync(id);
                if (car == null)
                    return NotFound();

                return View(car);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ EditCar GET error: {ex.Message}");
                TempData["Error"] = "An error occurred while loading the car details.";
                return RedirectToAction("ManageCars");
            }
        }

        // Edit Car (POST)
        [HttpPost]
        public async Task<IActionResult> EditCar(CarDto carDto)
        {
            try
            {
                if (!IsAdminAuthorized())
                    return RedirectIfNotAuthorized();

                if (!ModelState.IsValid)
                {
                    return View(carDto);
                }

                var (success, message) = await _adminService.UpdateCarAsync(carDto);
                
                if (success)
                {
                    TempData["Success"] = message;
                    return RedirectToAction("ManageCars");
                }
                else
                {
                    ModelState.AddModelError("", message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ EditCar POST error: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while updating the car.");
            }

            return View(carDto);
        }

        // Delete Car
        [HttpPost]
        public async Task<IActionResult> DeleteCar(int id)
        {
            try
            {
                if (!IsAdminAuthorized())
                    return RedirectIfNotAuthorized();

                var (success, message) = await _adminService.DeleteCarAsync(id);
                
                if (success)
                {
                    TempData["Success"] = message;
                }
                else
                {
                    TempData["Error"] = message;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ DeleteCar error: {ex.Message}");
                TempData["Error"] = "An error occurred while deleting the car.";
            }

            return RedirectToAction("ManageCars");
        }

        // Toggle Car Availability
        [HttpPost]
        public async Task<IActionResult> ToggleAvailability(int id)
        {
            try
            {
                if (!IsAdminAuthorized())
                    return Json(new { success = false, message = "Unauthorized" });

                var (success, message) = await _adminService.ToggleCarAvailabilityAsync(id);
                return Json(new { success = success, message = message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ToggleAvailability error: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while updating availability." });
            }
        }

        // View All Bookings
        //public async Task<IActionResult> ViewBookings()
        //{
        //    try
        //    {
        //        if (!IsAdminAuthorized())
        //            return RedirectIfNotAuthorized();

        //        var bookings = await _adminService.GetAllBookingsAsync();
        //        return View(bookings);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"❌ ViewBookings error: {ex.Message}");
        //        TempData["Error"] = "An error occurred while loading bookings.";
        //        return RedirectToAction("Dashboard");
        //    }
        //}

        // Update User Role
        [HttpPost]
        public async Task<IActionResult> UpdateUserRole(string userId, string newRole)
        {
            try
            {
                if (!IsAdminAuthorized())
                    return Json(new { success = false, message = "Unauthorized" });

                var (success, message) = await _adminService.UpdateUserRoleAsync(userId, newRole);
                return Json(new { success = success, message = message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ UpdateUserRole error: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while updating user role." });
            }
        }

        // Create User
        [HttpGet]
        public IActionResult CreateUser()
        {
            try
            {
                if (!IsAdminAuthorized())
                    return RedirectIfNotAuthorized();

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ CreateUser GET error: {ex.Message}");
                TempData["Error"] = "An error occurred while loading the create user page.";
                return RedirectToAction("Dashboard");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            try
            {
                if (!IsAdminAuthorized())
                    return RedirectIfNotAuthorized();

                if (ModelState.IsValid)
                {
                    var (success, message) = await _adminService.CreateUserAsync(model);
                    if (success)
                    {
                        TempData["Success"] = message;
                        return RedirectToAction("ManageUsers");
                    }
                    else
                    {
                        TempData["Error"] = message;
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ CreateUser POST error: {ex.Message}");
                TempData["Error"] = "An error occurred while creating the user.";
                return View(model);
            }
        }

        // Edit User
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            try
            {
                if (!IsAdminAuthorized())
                    return RedirectIfNotAuthorized();

                var user = await _adminService.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("ManageUsers");
                }

                var model = new EditUserViewModel
                {
                    Id = user.Id,
                    Username = user.UserName,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    SelectedRole = user.Role.ToString(),
                    AvailableRoles = Enum.GetNames<Car_Rent_System.Enums.Role>().ToList(),
                    CurrentRoles = new List<string> { user.Role.ToString() }
                };

                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ EditUser GET error: {ex.Message}");
                TempData["Error"] = "An error occurred while loading the user details.";
                return RedirectToAction("ManageUsers");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            try
            {
                if (!IsAdminAuthorized())
                    return RedirectIfNotAuthorized();

                if (ModelState.IsValid)
                {
                    var (success, message) = await _adminService.UpdateUserAsync(model);
                    if (success)
                    {
                        TempData["Success"] = message;
                        return RedirectToAction("ManageUsers");
                    }
                    else
                    {
                        TempData["Error"] = message;
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ EditUser POST error: {ex.Message}");
                TempData["Error"] = "An error occurred while updating the user.";
                return View(model);
            }
        }

        // Delete User
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                if (!IsAdminAuthorized())
                    return RedirectIfNotAuthorized();

                var (success, message) = await _adminService.DeleteUserAsync(id);
                if (success)
                {
                    TempData["Success"] = message;
                }
                else
                {
                    TempData["Error"] = message;
                }

                return RedirectToAction("ManageUsers");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ DeleteUser error: {ex.Message}");
                TempData["Error"] = "An error occurred while deleting the user.";
                return RedirectToAction("ManageUsers");
            }
        }

        // User Management
        [HttpGet]
        public async Task<IActionResult> UserManagement()
        {
            try
            {
                if (!IsAdminAuthorized())
                    return RedirectIfNotAuthorized();

                var users = await _adminService.GetAllUsersAsync();
                var userViewModels = users.Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Username = u.UserName,
                    FullName = u.FullName,
                    Email = u.Email,
                    Roles = new List<string> { u.Role.ToString() }
                }).ToList();

                return View(userViewModels);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ UserManagement error: {ex.Message}");
                TempData["Error"] = "An error occurred while loading users.";
                return RedirectToAction("Dashboard");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ClearAllCars()
        {
            try
            {
                if (!IsAdminAuthorized())
                    return RedirectIfNotAuthorized();

                var success = await _adminService.ClearAllCarsAsync();
                if (success)
                {
                    TempData["Success"] = "All cars have been cleared from the database.";
                }
                else
                {
                    TempData["Error"] = "Failed to clear cars from the database.";
                }

                return RedirectToAction("ManageCars");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ClearAllCars error: {ex.Message}");
                TempData["Error"] = "An error occurred while clearing cars.";
                return RedirectToAction("ManageCars");
            }
        }
    }
}