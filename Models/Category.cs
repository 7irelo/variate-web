using System.ComponentModel.DataAnnotations;

namespace variate.Models;

public class Category
{
    [Key] public int Id { get; set; }
    [Required] public required string Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
