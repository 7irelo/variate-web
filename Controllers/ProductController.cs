using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using variate.Data;
using variate.Mapping;
using variate.Models;

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

        // Helper method for logging errors and redirecting
        private IActionResult HandleError(Exception ex, string action, string message)
        {
            _logger.LogError(ex, message);
            return RedirectToAction("Error", "Home");
        }

        // GET: /products
        [HttpGet("")]
        public async Task<IActionResult> Index(string? searchString)
        {
            try
            {
                searchString = searchString?.ToLower();
                var products = await _db.Products
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Where(p => string.IsNullOrEmpty(searchString) ||
                        p.Name.ToLower().Contains(searchString) ||
                        p.Brand.ToLower().Contains(searchString) ||
                        p.Category.Name.ToLower().Contains(searchString))
                    .Select(p => p.ToDto())
                    .ToListAsync();

                return View(products);
            }
            catch (Exception e)
            {
                return HandleError(e, nameof(Index), "Error fetching products.");
            }
        }

        // GET: /products/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var product = await _db.Products
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Select(p => p.ToDto())
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    _logger.LogWarning("Product with ID {Id} not found.", id);
                    return NotFound();
                }

                ViewBag.ColorList = product.Colour?.Split(", ") ?? Array.Empty<string>();
                ViewBag.SizeList = product.Size?.Split(", ") ?? Array.Empty<string>();
                return View(product);
            }
            catch (Exception e)
            {
                return HandleError(e, nameof(Details), $"Error fetching product with ID {id}.");
            }
        }

        // GET: /products/create
        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            await PopulateCategoriesAsync();
            return View();
        }

        // POST: /products/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ValidateProduct(product))
            {
                await PopulateCategoriesAsync();
                return View(product);
            }

            try
            {
                await _db.Products.AddAsync(product);
                await _db.SaveChangesAsync();
                TempData["success"] = "Product created successfully";
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return HandleError(e, nameof(Create), "Error creating product.");
            }
        }

        // GET: /products/edit/{id}
        [HttpGet("edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                if (product == null)
                {
                    _logger.LogWarning("Product with ID {Id} not found.", id);
                    return NotFound();
                }

                await PopulateCategoriesAsync();
                return View(product);
            }
            catch (Exception e)
            {
                return HandleError(e, nameof(Edit), $"Error fetching product with ID {id}.");
            }
        }

        // POST: /products/edit/{id}
        [HttpPost("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product)
        {
            if (!ValidateProduct(product))
            {
                await PopulateCategoriesAsync();
                return View(product);
            }

            try
            {
                _db.Products.Update(product);
                await _db.SaveChangesAsync();
                TempData["success"] = "Product updated successfully";
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return HandleError(e, nameof(Edit), $"Error updating product with ID {product.Id}.");
            }
        }

        // GET: /products/delete/{id}
        [HttpGet("delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var product = await _db.Products
                    .AsNoTracking()
                    .Select(p => p.ToDto())
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    _logger.LogWarning("Product with ID {Id} not found.", id);
                    return NotFound();
                }

                return View(product);
            }
            catch (Exception e)
            {
                return HandleError(e, nameof(Delete), $"Error fetching product with ID {id}.");
            }
        }

        // POST: /products/delete/{id}
        [HttpPost("delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var product = await _db.Products.FindAsync(id);
                if (product == null)
                {
                    _logger.LogWarning("Product with ID {Id} not found for deletion.", id);
                    return NotFound();
                }

                _db.Products.Remove(product);
                await _db.SaveChangesAsync();
                TempData["success"] = "Product deleted successfully";
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return HandleError(e, nameof(DeleteConfirmed), $"Error deleting product with ID {id}.");
            }
        }

        private bool ValidateProduct(Product product)
        {
            if (product.Name.Contains(product.Brand, StringComparison.OrdinalIgnoreCase))
                ModelState.AddModelError("Name", "The Product Name should not contain the Brand Name.");

            if (product.CategoryId < 1)
                ModelState.AddModelError("CategoryId", "Please select a valid category.");

            return ModelState.IsValid;
        }

        private async Task PopulateCategoriesAsync()
        {
            ViewBag.Categories = await _db.Categories.AsNoTracking().ToListAsync();
        }
    }
}
