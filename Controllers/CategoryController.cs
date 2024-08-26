using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using variate.Data;
using variate.Models;

namespace variate.Controllers
{
    [Route("category")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CategoryController(ApplicationDbContext db)
        {
            _db = db;            
        }

        [HttpGet("{categoryName?}")]
        public IActionResult Index(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
            {
                // If no category name is provided, show the list of categories
                IEnumerable<Category> objCategoryList = _db.Categories.ToList();
                return View(objCategoryList);
            }

            // Convert the categoryName to the format used in the database
            string formattedCategoryName = ConvertToTitleCase(categoryName.Replace("-", " "));

            var category = _db.Categories
                            .Include(c => c.Products)
                            .FirstOrDefault(c => c.Name == formattedCategoryName);

            if (category == null)
            {
                return NotFound();
            }

            // Return the category details view
            return View("CategoryDetails", category);
        }

        // Helper method to convert the category name to Title Case
        private string ConvertToTitleCase(string input)
        {
            var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                // Skip "and" and capitalize all other words
                if (words[i].ToLower() != "and")
                {
                    words[i] = char.ToUpper(words[i][0]) + words[i][1..].ToLower();
                }
                else
                {
                    words[i] = words[i].ToLower();
                }
            }
            return string.Join(' ', words);
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
