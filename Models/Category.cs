using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace variate.Models;

[Table("categories")]
public class Category
{
    [Key]
    public int Id { get; set; }
    
    [Required, MaxLength(100)]
    public required string Name { get; set; }
    
    [MaxLength(600)]
    public string? Description { get; set; }
    
    [MaxLength(200)]
    public string? ImageUrl { get; set; }
    
    [MaxLength(100)]
    public string? MetaTitle { get; set; }
    
    [MaxLength(160)]
    public string? MetaDescription { get; set; }

    private ICollection<Product>? _products;
    public ICollection<Product> Products 
    {
        get => _products ??= new List<Product>(); 
        set => _products = value;
    }
}
