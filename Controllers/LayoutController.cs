using Car_Rent_System.Models;
using Microsoft.AspNetCore.Mvc;
using Car_Rent_System.Enum;

namespace Car_Rent_System.Controllers
{
    public class LayoutController : Controller
    {
        public IActionResult Sidebar()
        {
            var user = new User
            {
                FullName = "Admin User",
                Email = "admin@carrent.com",
                ProfileImageUrl = "/images/profile-placeholder.png",
                Role = Enum.UserRole.SuperAdmin
            };

            return PartialView("Sidebar", user);
        }

        public IActionResult Topbar()
        {
            var user = new User2
            {
                FullName = "Admin User",
                Email = "admin@carrent.com",
                ProfileImageUrl = "/images/profile-placeholder.png",
                Role = Ennum.UserRole.SuperAdmin
            };

            return PartialView("Topbar", user);
        }
    }

}
