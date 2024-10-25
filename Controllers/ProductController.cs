using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using variate.Data;
using variate.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace variate.Controllers
{
    [Route("products")]
    public class ProductController : Controller
    {
        private readonly ILogger<ProductController> _logger;
        private readonly ApplicationDbContext _db;

        public ProductController(ILogger<ProductController> logger, ApplicationDbContext db)
        {
            _logger = logger;
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

            return View(products);
        }

        // GET: /products/details/5
        [HttpGet("{id}")]
        public ActionResult Details(int id)
        {
            var product = _db.Products
                                .Include(p => p.Category)
                                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
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
        public IActionResult Create(Product product)
        {
            try
            {
                if (product.Name.Contains(product.Brand))
                {
                    ModelState.AddModelError("CustomError", "The Product Name should not contain the Brand Name");
                }
                else if (product.CategoryId < 1)
                {
                    ModelState.AddModelError("CustomError", "The Category ID cannot be less than zero");
                }

                if (ModelState.IsValid)
                {
                    _db.Products.Add(product);
                    _db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return View(product); 
            }
            catch
            {
                return View(product);
            }
        }

        // GET: /products/edit/5
        [HttpGet("edit/{id}")]
        public ActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var product = _db.Products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: /products/edit/5
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product product)
        {
            try
            {
                if (product.Name.Contains(product.Brand))
                {
                    ModelState.AddModelError("CustomError", "The Product Name should not contain the Brand Name");
                }
                else if (product.CategoryId < 1)
                {
                    ModelState.AddModelError("CustomError", "The Category ID cannot be less than zero");
                }

                if (ModelState.IsValid)
                {
                    _db.Products.Update(product);
                    _db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return View(product); 
            }
            catch
            {
                return View(product);
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
        public ActionResult DeleteP(int id)
        {
            try
            {
                var product = _db.Products.FirstOrDefault(p => p.Id == id);
                if (product == null)
                {
                    return NotFound();
                }
                
                _db.Products.Remove(product);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
