using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using variate.Data;
using variate.Dtos;
using variate.Mapping;

namespace variate.Controllers
{
    [Route("categories")]
    public class CategoryController : Controller
    {
        private readonly ILogger<CategoryController> _logger;
        private readonly ApplicationDbContext _db;

        public CategoryController(ILogger<CategoryController> logger, ApplicationDbContext db)
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

        // Fetch all categories with their products
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var categories = await _db.Categories
                    .AsNoTracking()
                    .Include(c => c.Products)
                    .Select(c => c.ToDto())
                    .ToListAsync();

                if (!categories.Any())
                {
                    _logger.LogWarning("No categories found.");
                    return NotFound();
                }

                return View(categories);
            }
            catch (Exception e)
            {
                return HandleError(e, nameof(Index), "Error fetching categories.");
            }
        }

        // Fetch all products in a specific category by category ID
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var products = await _db.Products
                    .AsNoTracking()
                    .Where(p => p.CategoryId == id)
                    .Select(p => p.ToDto())
                    .ToListAsync();

                if (!products.Any())
                {
                    _logger.LogWarning("No products found in category with ID {Id}", id);
                    return NotFound();
                }

                return View(products);
            }
            catch (Exception e)
            {
                return HandleError(e, nameof(Details), $"Error fetching products from category with ID {id}.");
            }
        }

        // Create category (GET)
        [HttpGet("create")]
        public IActionResult Create() => View();

        // Create category (POST)
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryDto obj)
        {
            if (!ModelState.IsValid) return View(obj);

            try
            {
                await _db.Categories.AddAsync(obj.ToEntity());
                await _db.SaveChangesAsync();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return HandleError(e, nameof(Create), "Error creating category.");
            }
        }

        // Edit category by ID (GET)
        [HttpGet("edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var category = await _db.Categories
                    .AsNoTracking()
                    .Where(c => c.Id == id)
                    .Select(c => c.ToDto())
                    .FirstOrDefaultAsync();

                if (category == null)
                {
                    _logger.LogWarning("Category with ID {Id} not found.", id);
                    return NotFound();
                }

                return View(category);
            }
            catch (Exception e)
            {
                return HandleError(e, nameof(Edit), $"Error fetching category with ID {id}.");
            }
        }

        // Edit category by ID (POST)
        [HttpPost("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryDto obj, int id)
        {
            if (!ModelState.IsValid || obj.Id != id)
            {
                _logger.LogWarning("Invalid ModelState or ID mismatch for editing category with ID {Id}.", id);
                return View(obj);
            }

            try
            {
                _db.Categories.Update(obj.ToEntity());
                await _db.SaveChangesAsync();
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return HandleError(e, nameof(Edit), $"Error updating category with ID {id}.");
            }
        }

        // Delete category by ID (GET)
        [HttpGet("delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var category = await _db.Categories
                    .AsNoTracking()
                    .Where(c => c.Id == id)
                    .Select(c => c.ToDto())
                    .FirstOrDefaultAsync();

                if (category == null)
                {
                    _logger.LogWarning("Category with ID {Id} not found.", id);
                    return NotFound();
                }

                return View(category);
            }
            catch (Exception e)
            {
                return HandleError(e, nameof(Delete), $"Error fetching category with ID {id}.");
            }
        }

        // Delete category by ID (POST)
        [HttpPost("delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var category = await _db.Categories.FindAsync(id);
                
                if (category == null)
                {
                    _logger.LogWarning("Category with ID {Id} not found for deletion.", id);
                    return NotFound();
                }

                _db.Categories.Remove(category);
                await _db.SaveChangesAsync();
                TempData["success"] = "Category deleted successfully";
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return HandleError(e, nameof(DeleteConfirmed), $"Error deleting category with ID {id}.");
            }
        }
    }
}
