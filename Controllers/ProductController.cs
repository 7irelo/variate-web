using Microsoft.AspNetCore.Mvc;
using Variate.Data;
using Variate.Dto;
using Variate.Models;
using Variate.Mapping;

namespace Variate.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Product
    [HttpGet]
    public IActionResult GetProducts()
    {
        var products = _context.Products.Select(p => p.ToProductSummaryDto()).ToList();
        return Ok(products);
    }

    // GET: api/Product/5
    [HttpGet("{id}")]
    public IActionResult GetProduct(int id)
    {
        var product = _context.Products.Find(id);
        if (product == null)
        {
            return NotFound();
        }
        return Ok(product.ToProductDetailsDto());
    }

    // POST: api/Product
    [HttpPost]
    public IActionResult CreateProduct([FromBody] CreateProductDto productDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var product = productDto.ToEntity();

        _context.Products.Add(product);
        _context.SaveChanges();

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product.ToProductDetailsDto());
    }

    // PUT: api/Product/5
    [HttpPut("{id}")]
    public IActionResult UpdateProduct(int id, [FromBody] UpdateProductDto productDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var product = _context.Products.Find(id);
        if (product == null)
        {
            return NotFound();
        }

        product.Name = productDto.Name;
        product.CategoryId = productDto.CategoryId;
        product.Price = productDto.Price;
        product.Release = productDto.Release;

        _context.Products.Update(product);
        _context.SaveChanges();

        return NoContent();
    }

    // PATCH: api/Product/5
    [HttpPatch("{id}")]
    public IActionResult PatchProduct(int id, [FromBody] UpdateProductDto productDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var product = _context.Products.Find(id);
        if (product == null)
        {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(productDto.Name))
        {
            product.Name = productDto.Name;
        }

        if (productDto.CategoryId != 0)
        {
            product.CategoryId = productDto.CategoryId;
        }

        if (productDto.Price != 0)
        {
            product.Price = productDto.Price;
        }

        if (productDto.Release != default)
        {
            product.Release = productDto.Release;
        }

        _context.Products.Update(product);
        _context.SaveChanges();

        return NoContent();
    }

    // DELETE: api/Product/5
    [HttpDelete("{id}")]
    public IActionResult DeleteProduct(int id)
    {
        var product = _context.Products.Find(id);
        if (product == null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);
        _context.SaveChanges();

        return NoContent();
    }
}
