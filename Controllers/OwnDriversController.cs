`using AspNetCoreGeneratedDocument;
using Car_Rent_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car_Rent_System.Controllers
{
    public class OwnDriversController : Controller
    {
        private readonly DbContextOptions context;
        

        public OwnDriversController(DbContextOptions _context)
        {
            context = _context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]

        public IActionResult Create(OwnDrivers ownDrivers)
        {
            
            return View(ownDrivers);

        }

        [HttpGet]

        public IActionResult Dashboard()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Dashboard(OwnDrivers ownDrivers)
        {
            return View(ownDrivers);
        }
    }
}
