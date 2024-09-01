using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using variate.Data;
using variate.Models;
using System.Linq;

namespace variate.Controllers
{
    [Route("categories")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CategoryController(ApplicationDbContext db)
        {
            _db = db;            
        }

        [HttpGet]
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

        [HttpGet("{categoryName}")]
        public IActionResult Details(string categoryName)
        {
            // Transform the URL segment to the appropriate category name format
            string formattedCategoryName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(categoryName.Replace("-", " "));
            
            // Exception handling for the word "and" (ensure "and" is not capitalized)
            formattedCategoryName = formattedCategoryName.Replace(" And ", " and ");

            // Fetch the category by name including its products
            var category = _db.Categories
                            .Include(c => c.Products)
                            .FirstOrDefault(c => c.Name == formattedCategoryName);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category obj)
        {
            if (ModelState.IsValid)
            {
                _db.Categories.Add(obj);
                _db.SaveChanges();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        [HttpGet("edit/{id}")]
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var categoryFromDb = _db.Categories.FirstOrDefault(c => c.Id == id);

            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                _db.Categories.Update(obj);
                _db.SaveChanges();
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        [HttpGet("delete/{id}")]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var categoryFromDb = _db.Categories.FirstOrDefault(c => c.Id == id);

            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {
            var obj = _db.Categories.Find(id);
            if (obj == null)
            {
                return NotFound();
            }
            _db.Categories.Remove(obj);
            _db.SaveChanges();
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
