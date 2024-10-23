using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using variate.Data;
using variate.Models;
using System.Linq;

namespace variate.Controllers
{
    [Route("cart")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CartController(ApplicationDbContext db)
        {
            _db = db;            
        }

        [HttpGet]
        public IActionResult Index()
        {
            var categoriesWithProducts = _db.Categories
                                            .Include(c => c.Products)
                                            .ToList();
            return View(categoriesWithProducts);
        }
    }
}