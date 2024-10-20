using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // Fetch all categories with their products
        [HttpGet]
        public IActionResult Index()
        {
            // Fetch categories for the sidebar
            var categories = _db.Categories.ToList();
            ViewBag.Categories = categories;

            // Fetch categories along with their products
            var categoriesWithProducts = _db.Categories
                                            .Include(c => c.Products)
                                            .ToList();

            return View(categoriesWithProducts);
        }

        // Fetch category by ID and its products
        [HttpGet("{id:int}")]
        public IActionResult Details(int id)
        {
            // Fetch the category by ID including its products
            var category = _db.Categories
                              .Include(c => c.Products)
                              .FirstOrDefault(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // Create category (GET)
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }

        // Create category (POST)
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

        // Edit category by ID (GET)
        [HttpGet("{id:int}/edit")]
        public IActionResult Edit(int id)
        {
            var categoryFromDb = _db.Categories.FirstOrDefault(c => c.Id == id);

            if (categoryFromDb == null)
            {
                return NotFound();
            }

            return View(categoryFromDb);
        }

        // Edit category by ID (POST)
        [HttpPost("{id:int}/edit")]
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

        // Delete category by ID (GET)
        [HttpGet("{id:int}/delete")]
        public IActionResult Delete(int id)
        {
            var categoryFromDb = _db.Categories.FirstOrDefault(c => c.Id == id);

            if (categoryFromDb == null)
            {
                return NotFound();
            }

            return View(categoryFromDb);
        }

        // Delete category by ID (POST)
        [HttpPost("{id:int}/delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int id)
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
