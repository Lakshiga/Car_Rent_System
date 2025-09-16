using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using Car_Rent_System.Data;
using Car_Rent_System.DTOs;
using Car_Rent_System.Enums;
using Car_Rent_System.Interfaces;
using Car_Rent_System.Mappers;
using Car_Rent_System.Models;
using Car_Rent_System.Repositories;
using Car_Rent_System.Services;
using Car_Rent_System.Services.Interfaces;
using Car_Rent_System.ViewModels.Account;
using CloudinaryDotNet;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Car_Rent_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountApiController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;

        public AccountApiController(IUserService userService, IJwtService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> ApiLogin([FromBody] LoginDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                    return BadRequest(new { error = "Email and password required." });

                var user = await _userService.AuthenticateAsync(request.Email, request.Password)
                    ?? await _userService.AuthenticateByUsernameAsync(request.Email, request.Password);

                if (user == null)
                    return Unauthorized(new { error = "Invalid credentials." });

                if (user.VerificationStatus != VerificationStatus.Approved)
                    return Unauthorized(new { error = "Account not approved by admin." });

                var token = _jwtService.GenJwtToken(user);
                if (string.IsNullOrWhiteSpace((string?)token))
                    return StatusCode(500, new { error = "Failed to generate token." });

                return Ok(new
                {
                    message = "Login successful",
                    token,
                    role = user.Role.ToString(),
                    username = user.Email,
                    fullName = user.FullName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Server error: {ex.Message}", detail = ex.ToString() });
            }
        }
    }

    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IUserService userService,
            IEmailService emailService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _userService = userService;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (!string.IsNullOrEmpty(userId))
                {
                    var user = _userService.GetUserByIdAsync(userId).Result;
                    if (user != null)
                    {
                        var userRoles = _userManager.GetRolesAsync(user).Result;
                        var userRole = userRoles.FirstOrDefault();

                        if (!string.IsNullOrEmpty(userRole) && Enum.TryParse<Car_Rent_System.Enums.Role>(userRole, true, out var parsedRole))
                        {
                            return RedirectToAction("Dashboard", GetDashboardController(parsedRole));
                        }
                        // Fallback to Customer if role is not valid
                        return RedirectToAction("Dashboard", GetDashboardController(Car_Rent_System.Enums.Role.Customer));
                    }
                }

                return View(new LoginViewModel());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Login GET error: {ex.Message}");
                return View(new LoginViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Use service layer for login
                var (success, message, user) = await _userService.LoginUserAsync(model.EmailOrUsername, model.Password, model.RememberMe);

                if (!success)
                {
                    TempData["Error"] = message;
                    return View(model);
                }

                // Sign in user
                await _signInManager.SignInAsync(user, isPersistent: model.RememberMe);

                // Set session data
                HttpContext.Session.SetString("UserId", user.Id ?? "");
                HttpContext.Session.SetString("Username", user.UserName ?? "");
                HttpContext.Session.SetString("Email", user.Email ?? "");
                HttpContext.Session.SetString("Role", (user.Role != null ? user.Role : Car_Rent_System.Enums.Role.Customer).ToString());
                HttpContext.Session.SetString("FullName", user.FullName ?? "");
                HttpContext.Session.SetString("ImageUrl", user.ImageUrl ?? "");

                TempData["Success"] = $"Welcome back, {user.FullName}!";

                // Redirect based on role
                if (user.Role == Car_Rent_System.Enums.Role.Admin || user.Role == Car_Rent_System.Enums.Role.SubAdmin || user.Role == Car_Rent_System.Enums.Role.Staff)
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                else if (user.Role == Car_Rent_System.Enums.Role.Customer)
                {
                    return RedirectToAction("Dashboard", "Customer");
                }
                else if (user.Role == Car_Rent_System.Enums.Role.Driver)
                {
                    return RedirectToAction("Dashboard", "Driver");
                }
                else if (user.Role == Car_Rent_System.Enums.Role.VehicleOwner)
                {
                    return RedirectToAction("Dashboard", "VehicleOwner");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Login POST error: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            try
            {
                if (!model.AgreeTerms)
                {
                    ModelState.AddModelError("AgreeTerms", "You must agree to the Terms of Service and Privacy Policy.");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Use service layer for registration
                var (success, message, user) = await _userService.RegisterUserAsync(model);

                if (!success)
                {
                    ModelState.AddModelError(string.Empty, message);
                    return View(model);
                }

                // Handle successful registration
                if (user.VerificationStatus == VerificationStatus.Approved)
                {
                    // Auto-login for customers
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    HttpContext.Session.SetString("UserId", user.Id);
                    HttpContext.Session.SetString("Username", user.UserName);
                    HttpContext.Session.SetString("Email", user.Email);
                    HttpContext.Session.SetString("Role", user.Role.ToString());
                    HttpContext.Session.SetString("FullName", user.FullName);
                    HttpContext.Session.SetString("ImageUrl", user.ImageUrl ?? "");

                    TempData["Success"] = $"Welcome to CYNEX BLAZER, {user.FullName}!";
                    return RedirectToAction("Dashboard", "Customer");
                }
                else
                {
                    // Send email confirmation for driver/owner
                    try
                    {
                        var token = await _userService.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token }, Request.Scheme);

                        await _emailService.SendEmailAsync(user.Email, "Confirm Your CYNEX BLAZER Account",
                            $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>.<br/>" +
                            $"Your {model.RequestedRole} application will be reviewed by admin shortly.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Email failed: {ex.Message}");
                        TempData["Warning"] = "Registration succeeded, but email failed. Contact support.";
                    }

                    TempData["Success"] = "Registration successful! Please wait for admin approval.";
                    return RedirectToAction("Login");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Registration error: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
                return View(model);
            }
        }


        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login");
                }

                var success = await _userService.ConfirmEmailAsync(userId, token);
                if (success)
                {
                    ViewBag.Message = "Thank you for confirming your email. Please wait for admin approval.";
                    return View("Info");
                }

                ViewBag.Error = "Email confirmation failed.";
                return View("Info");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ConfirmEmail error: {ex.Message}");
                ViewBag.Error = "An error occurred during email confirmation.";
                return View("Info");
            }
        }

        public async Task<IActionResult> Logout()
        {
            try
            {
                await _userService.SignOutAsync();
                HttpContext.Session.Clear();
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Logout error: {ex.Message}");
                HttpContext.Session.Clear();
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login");
                }

                var model = await _userService.GetUserProfileAsync(userId);
                if (model == null)
                {
                    return RedirectToAction("Login");
                }

                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Profile GET error: {ex.Message}");
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Profile(ProfileDto model, IFormFile? ImageFile)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Use service layer for profile update
                var (success, message) = await _userService.UpdateUserProfileAsync(userId, model, ImageFile);

                if (success)
                {
                    // Update session data
                    HttpContext.Session.SetString("FullName", model.FullName);
                    HttpContext.Session.SetString("ImageUrl", model.ImageUrl ?? "");

                    TempData["Success"] = message;
                    return RedirectToAction("Profile");
                }

                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Profile POST error: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while updating your profile. Please try again.");
                return View(model);
            }
        }

        private string GetDashboardController(Role? role = null)
        {
            var sessionRole = HttpContext.Session.GetString("Role");
            if (role == null && !string.IsNullOrEmpty(sessionRole) && Enum.TryParse<Role>(sessionRole, true, out var parsed))
            {
                role = parsed;
            }

            role ??= Car_Rent_System.Enums.Role.Customer;

            return role switch
            {
                Car_Rent_System.Enums.Role.Admin or Car_Rent_System.Enums.Role.SubAdmin or Car_Rent_System.Enums.Role.Staff => "Admin",
                Car_Rent_System.Enums.Role.Customer => "Customer",
                Car_Rent_System.Enums.Role.Driver => "Driver",
                Car_Rent_System.Enums.Role.VehicleOwner => "VehicleOwner",
                _ => "Home"
            };
        }
    }
}
