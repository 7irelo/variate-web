using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Variate.Models;

namespace Variate.Controllers
{
    public class ProductController : Controller
    {
        private readonly ILogger<ProductController> _logger;
        private readonly ApplicationDbContext _db;

        public ProductController(ILogger<ProductController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        [Route("products")]
        public IActionResult Index()
        {
            IEnumerable<Product> objProductList = _db.Products;
            return View(objProductList);
        }

        [Route("products/create-product")]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [Route("products/create-product")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product obj)
        {
           if(ModelState.IsValid)
           {
               _db.Products.Add(obj);
               _db.SaveChanges();
               return RedirectToAction("Index");
           }
           return View(obj);
        }

        [Route("products/{id}")]
        public IActionResult Item(int? id)
        {
            if(id == null || id == 0)
            {
                return NotFound();
            }
            var product = _db.Products.Find(id);
            if(product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [Route("products/edit-product/{id}")]
        public IActionResult Edit(int? id)
        {
            if(id == null || id == 0)
            {
                return NotFound();
            }
            var product = _db.Products.Find(id);
            if(product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        [HttpPut]
        [Route("products/edit-product/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product obj)
        {
           if(ModelState.IsValid)
           {
               _db.Products.Update(obj);
               _db.SaveChanges();
               return RedirectToAction("Index");
           }
           return View(obj);
        }

        [Route("products/delete-product/{id}")]
        public IActionResult Delete(int? id)
        {
            if(id == null || id == 0)
            {
                return NotFound();
            }
            var product = _db.Products.Find(id);
            if(product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        [HttpDelete]
        [Route("products/delete-product/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
           var product = _db.Products.Find(id);
           if(product == null)
           {
               return NotFound();
           }
           _db.Products.Remove(product);
           _db.SaveChanges();
           return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
