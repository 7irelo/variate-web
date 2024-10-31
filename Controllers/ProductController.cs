using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using variate.Mapping;
using variate.Models;
using variate.Services;

namespace variate.Controllers
{
    [Route("products")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchString)
        {
            var products = await _productService.GetAllProductsAsync(searchString);
            return View(products);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductDetailsAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {Id} not found.", id);
                return NotFound(new { Message = "Product not found" });
            }

            return View(product.ToDto());
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _productService.CreateProductAsync(product);
            if (!result)
            {
                _logger.LogError("Failed to create product.");
                return StatusCode(500, "Internal server error while creating product");
            }

            return CreatedAtAction(nameof(Details), new { id = product.Id }, product);
        }

        [HttpGet("edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductForEditAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {Id} not found.", id);
                return NotFound();
            }

            return View(product);
        }

        [HttpPost("{id:int}")]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (!ModelState.IsValid || product.Id != id)
            {
                return BadRequest(ModelState);
            }

            var result = await _productService.UpdateProductAsync(product);
            if (!result)
            {
                _logger.LogError("Failed to update product with ID {Id}", id);
                return StatusCode(500, "Internal server error while updating product");
            }

            return RedirectToAction("Details", "Product", new { id = id} );
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result)
            {
                _logger.LogWarning("Product with ID {Id} not found for deletion.", id);
                return NotFound();
            }

            return NoContent();
        }
    }
}
