using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using variate.Data;
using variate.Models;

namespace variate.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db; 
        }

        public IActionResult Index()
        {
            // Fetch categories for the sidebar
            var categories = _db.Categories.ToList();
            ViewBag.Categories = categories;

            // Fetch categories along with their products to display on the page
            var categoriesWithProducts = _db.Categories
                                            .Include(c => c.Products)
                                            .ToList();

            return View(categoriesWithProducts);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
