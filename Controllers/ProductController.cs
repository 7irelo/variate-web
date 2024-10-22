using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using variate.Data;
using variate.Models;
using System.Linq;

namespace variate.Controllers
{
    [Route("products")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ProductController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /products
        [HttpGet("")]
        public ActionResult Index(string searchString)
        {
            var products = _db.Products.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();

                // Search by product name or category name
                products = products.Where(p => p.Name.ToLower().Contains(searchString) ||
                                                p.Category.Name.ToLower().Contains(searchString));
            }

            return View(products.ToList());
        }

        // GET: /products/details/5
        [HttpGet("details/{id}")]
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: /products/create
        [HttpGet("create")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: /products/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: /products/edit/5
        [HttpGet("edit/{id}")]
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: /products/edit/5
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: /products/delete/5
        [HttpGet("delete/{id}")]
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: /products/delete/5
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
