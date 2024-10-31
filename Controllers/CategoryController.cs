using Microsoft.AspNetCore.Mvc;
using variate.Dtos;
using variate.Services;

namespace variate.Controllers
{
    [Route("categories")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            if (!categories.Any())
            {
                _logger.LogWarning("No categories found.");
                return NotFound();
            }

            return View(categories);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var products = await _categoryService.GetProductsByCategoryIdAsync(id);
            if (!products.Any())
            {
                _logger.LogWarning("No products found in category with ID {Id}.", id);
                return NotFound();
            }

            return View(products);
        }

        [HttpGet("create")]
        public IActionResult Create() => View();

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryDto categoryDto)
        {
            if (!ModelState.IsValid) return View(categoryDto);

            var result = await _categoryService.CreateCategoryAsync(categoryDto);
            if (!result)
            {
                _logger.LogError("Failed to create category.");
                return BadRequest("Error creating category.");
            }

            TempData["success"] = "Category created successfully";
            return RedirectToAction("Index");
        }

        [HttpGet("edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Category with ID {Id} not found.", id);
                return NotFound();
            }

            return View(category);
        }

        [HttpPost("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryDto categoryDto, int id)
        {
            if (!ModelState.IsValid || categoryDto.Id != id)
            {
                _logger.LogWarning("Invalid ModelState or ID mismatch for editing category with ID {Id}.", id);
                return View(categoryDto);
            }

            var result = await _categoryService.UpdateCategoryAsync(categoryDto);
            if (!result)
            {
                _logger.LogError("Failed to update category with ID {Id}.", id);
                return BadRequest("Error updating category.");
            }

            TempData["success"] = "Category updated successfully";
            return RedirectToAction("Index");
        }

        [HttpGet("delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Category with ID {Id} not found.", id);
                return NotFound();
            }

            return View(category);
        }

        [HttpPost("delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            if (!result)
            {
                _logger.LogError("Failed to delete category with ID {Id}.", id);
                return BadRequest("Error deleting category.");
            }

            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
