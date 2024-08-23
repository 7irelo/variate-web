using Microsoft.AspNetCore.Mvc;
using Variate.Data;
using Variate.Dto;
using Variate.Models;
using Variate.Mapping;

namespace Variate.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CategoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Category
    [HttpGet]
    public IActionResult GetCategories()
    {
        var categories = _context.Categories.Select(c => c.ToDto()).ToList();
        return Ok(categories);
    }

    // GET: api/Category/5
    [HttpGet("{id}")]
    public IActionResult GetCategory(int id)
    {
        var category = _context.Categories.Find(id);
        if (category == null)
        {
            return NotFound();
        }
        return Ok(category.ToDto());
    }

    // POST: api/Category
    [HttpPost]
    public IActionResult CreateCategory([FromBody] CategoryDto categoryDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var category = new Category
        {
            Name = categoryDto.Name
        };

        _context.Categories.Add(category);
        _context.SaveChanges();

        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category.ToDto());
    }

    // PUT: api/Category/5
    [HttpPut("{id}")]
    public IActionResult UpdateCategory(int id, [FromBody] CategoryDto categoryDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var category = _context.Categories.Find(id);
        if (category == null)
        {
            return NotFound();
        }

        category.Name = categoryDto.Name;

        _context.Categories.Update(category);
        _context.SaveChanges();

        return NoContent();
    }

    // PATCH: api/Category/5
    [HttpPatch("{id}")]
    public IActionResult PatchCategory(int id, [FromBody] CategoryDto categoryDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var category = _context.Categories.Find(id);
        if (category == null)
        {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(categoryDto.Name))
        {
            category.Name = categoryDto.Name;
        }

        _context.Categories.Update(category);
        _context.SaveChanges();

        return NoContent();
    }

    // DELETE: api/Category/5
    [HttpDelete("{id}")]
    public IActionResult DeleteCategory(int id)
    {
        var category = _context.Categories.Find(id);
        if (category == null)
        {
            return NotFound();
        }

        _context.Categories.Remove(category);
        _context.SaveChanges();

        return NoContent();
    }
}
