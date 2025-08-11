using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Car_Rent_System.Models;
using Car_Rent_System.Services;  
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Car_Rent_System.Controllers
{
  
    [ApiController]
    [Route("api/[controller]")]
    public class AccountApiController : ControllerBase
    {
        private readonly CynexBlazerContext _context;
        private readonly IConfiguration _config;

        public AccountApiController(CynexBlazerContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        

        [HttpPost("login")]
        public async Task<IActionResult> ApiLogin([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                return BadRequest(new { error = "Please enter both username and password." });

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == request.Password && u.IsActive);

            if (user != null)
            {
                var token = JwtTokenHelper.GenerateJwtToken(user, _config);
                return Ok(new { message = "Login successful", token, role = user.Role, username = user.Username });
            }

            return Unauthorized(new { error = "Invalid username or password." });
        }


    }


    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly CynexBlazerContext _context;
        private readonly IConfiguration _config;

        public AccountController(CynexBlazerContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please enter both username and password.";
                return View();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password && u.IsActive);

            if (user != null)
            {
                var token = JwtTokenHelper.GenerateJwtToken(user, _config);

                HttpContext.Session.SetInt32("UserID", user.UserID);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role);
                HttpContext.Session.SetString("FullName", user.FullName ?? user.Username);
                HttpContext.Session.SetString("Token", token);

                TempData["Success"] = "Login successful!";

                if (user.Role == "Admin")
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                else if (user.Role == "Customer")
                {
                    return RedirectToAction("Dashboard", "Customer");
                }
                else if (user.Role == "Staff")
                {
                    return RedirectToAction("Dashboard", "Staff");
                }

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid username or password.";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginSessionSet([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                return BadRequest();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == request.Password && u.IsActive);

            if (user != null)
            {
                HttpContext.Session.SetInt32("UserID", user.UserID);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role);
                HttpContext.Session.SetString("FullName", user.FullName ?? user.Username);
                return Ok(new { message = "Session set" });
            }

            return Unauthorized();
        }



        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == user.Username);

                if (existingUser != null)
                {
                    ViewBag.Error = "Username already exists. Please choose a different username.";
                    return View(user);
                }

                var existingEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == user.Email);

                if (existingEmail != null)
                {
                    ViewBag.Error = "Email already registered. Please use a different email.";
                    return View(user);
                }

                user.Role = "Customer"; 
                user.DateJoined = System.DateTime.Now;
                user.IsActive = true;

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Registration successful! Please login with your credentials.";
                return RedirectToAction("Login");
            }

            return View(user);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return RedirectToAction("Login");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return RedirectToAction("Login");

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(User user)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return RedirectToAction("Login");

            if (ModelState.IsValid)
            {
                var existingUser = await _context.Users.FindAsync(userId);
                if (existingUser != null)
                {
                    existingUser.FullName = user.FullName;
                    existingUser.Email = user.Email;
                    existingUser.PhoneNumber = user.PhoneNumber;

                    await _context.SaveChangesAsync();
                    ViewBag.Success = "Profile updated successfully!";
                }
            }

            return View(user);
        }
    }

    // DTO for API login request
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}