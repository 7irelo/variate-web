using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace variate.Models;

[Table("categories")]
public class Category
{
    [Key]
    public int Id { get; set; }
    
    [Required, MaxLength(100)]
    public string Name { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(200)]
    public string? ImageUrl { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
