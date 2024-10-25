using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace variate.Models;

[Table("products")]
public class Product
{
    [Key]
    [DisplayName("Product ID")]
    public int Id { get; set; }
    
    [DisplayName("Category ID")]
    public int CategoryId { get; set; }
    
    public Category? Category { get; set; }

    [Required, MaxLength(100)]
    public string? Name { get; set; }
    
    [Required, MaxLength(200)]
    public string? Description { get; set; }
    
    [MaxLength(50)]
    public string? Colour { get; set; }
    
    [MaxLength(50)]
    public string? Size { get; set; }
    
    [Range(0, 100000)]
    public decimal Price { get; set; }

    [Range(0, 100000)]
    public decimal? DiscountedPrice { get; set; }
    
    [DisplayName("Release Date")]
    public DateOnly Release { get; set; }
    
    [Required, MaxLength(200)]
    [DisplayName("Image URL")]
    public string? ImageUrl { get; set; }

    [MaxLength(100)]
    [DisplayName("Stock Keeping Unit")]
    public string? SKU { get; set; } // Stock Keeping Unit
    
    [Range(0, 10000)]
    public int? Stock { get; set; }

    [DefaultValue(false)]
    [DisplayName("Is Featured")]
    public bool IsFeatured { get; set; } = false;

    [DefaultValue(false)]
    [DisplayName("On Sale")]
    public bool OnSale { get; set; } = false;

    [MaxLength(200)]
    public string? Brand { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}
