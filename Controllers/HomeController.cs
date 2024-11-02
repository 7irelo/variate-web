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

        // Index action to fetch products on sale
        public async Task<IActionResult> Index()
        {
            try
            {
                // Log that we are fetching products on sale
                _logger.LogInformation("Fetching products that are on sale");

                // Fetch all products where OnSale is true asynchronously
                var productsOnSale = await _db.Products
                                              .Where(p => p.OnSale)
                                              .ToListAsync();

                // Log the number of products fetched
                _logger.LogInformation("{Count} products found on sale", productsOnSale.Count);

                // Pass the list of products to the view
                return View(productsOnSale);
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "Error occurred while fetching products on sale");

                // Optionally, redirect to an error page or return an error view
                return RedirectToAction("Error");
            }
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
