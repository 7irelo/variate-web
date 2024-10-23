using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace variate.Models;

[Table("products")]
public class Product
{
    [Key]
    public int Id { get; set; }
    
    [DisplayName("Category ID")]
    public int CategoryId { get; set; }
    
    public Category? Category { get; set; }

    [Required, MaxLength(100)]
    public string? Name { get; set; }
    
    [Required, MaxLength(200)]
    public string? Description { get; set; }
    
    [Range(0, 100000)]
    public decimal Price { get; set; }

    [Range(0, 100000)]
    public decimal? DiscountedPrice { get; set; }
    
    public DateOnly Release { get; set; }
    
    [Required, MaxLength(200)]
    public string? ImageUrl { get; set; }

    [MaxLength(100)]
    public string? SKU { get; set; } // Stock Keeping Unit
    
    [Range(0, 10000)]
    public int? Stock { get; set; }

    [DefaultValue(false)]
    public bool? IsFeatured { get; set; } = false;

    [DefaultValue(false)]
    public bool OnSale { get; set; } = false;

    [MaxLength(200)]
    public string? Brand { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}
