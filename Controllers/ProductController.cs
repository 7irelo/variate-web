using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using variate.Data;
using variate.Models;
using System.Linq;
using Microsoft.Extensions.Logging;

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
        public IActionResult Index(string searchString)
        {
            var products = _db.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                products = products.Where(p => p.Name.ToLower().Contains(searchString) ||
                                                p.Category.Name.ToLower().Contains(searchString));
            }

            return View(products);
        }

        // GET: /products/details/5
        [HttpGet("{id}")]
        public IActionResult Details(int id)
        {
            var product = _db.Products
                             .Include(p => p.Category)
                             .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                _logger.LogWarning("Product with ID {Id} not found.", id);
                return NotFound();
            }

            ViewData["ColorList"] = product.Colour?.Split(", ") ?? new string[0];
            ViewData["SizeList"] = product.Size?.Split(", ") ?? new string[0];

            return View(product);
        }

        // GET: /products/create
        [HttpGet("create")]
        public IActionResult Create()
        {
            PopulateCategories();
            return View();
        }

        // POST: /products/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product)
        {
            if (ValidateProduct(product))
            {
                try
                {
                    _db.Products.Add(product);
                    _db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating product.");
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the product.");
                }
            }

            PopulateCategories();
            return View(product);
        }

        // GET: /products/edit/5
        [HttpGet("edit/{id}")]
        public IActionResult Edit(int id)
        {
            var product = _db.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {Id} not found.", id);
                return NotFound();
            }

            PopulateCategories();
            return View(product);
        }

        // POST: /products/edit/5
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product product)
        {
            if (ValidateProduct(product))
            {
                try
                {
                    _db.Products.Update(product);
                    _db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error editing product with ID {Id}.", product.Id);
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the product.");
                }
            }

            PopulateCategories();
            return View(product);
        }

        // GET: /products/delete/5
        [HttpGet("delete/{id}")]
        public IActionResult Delete(int id)
        {
            var product = _db.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {Id} not found.", id);
                return NotFound();
            }
            return View(product);
        }

        // POST: /products/delete/5
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var product = _db.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {Id} not found.", id);
                return NotFound();
            }

            try
            {
                _db.Products.Remove(product);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID {Id}.", id);
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the product.");
                return View(product);
            }
        }

        private bool ValidateProduct(Product product)
        {
            if (product.Name.Contains(product.Brand))
            {
                ModelState.AddModelError("Name", "The Product Name should not contain the Brand Name");
            }
            if (product.CategoryId < 1)
            {
                ModelState.AddModelError("CategoryId", "The Category ID cannot be less than zero");
            }

            return ModelState.IsValid;
        }

        private void PopulateCategories()
        {
            ViewData["Categories"] = _db.Categories.ToList();
        }
    }
}
