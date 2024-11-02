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
    
    [Required, MaxLength(200)]
    public required string Brand { get; set; }

    [Required, MaxLength(100)]
    public required string Name { get; set; }
    
    [Required, MaxLength(200)]
    public string? Description { get; set; }
    
    [MaxLength(50)]
    public string? Colour { get; set; }
    
    [MaxLength(50)]
    public string? Size { get; set; }
    
    [Range(0, 100000)]
    public decimal Price { get; set; }

    [Range(0, 100000)]
    public decimal DiscountedPrice { get; set; }
    
    [DisplayName("Release Date")]
    public DateOnly ReleaseDate { get; set; }
    
    [MaxLength(200)]
    [DisplayName("Image URL")]
    public string? ImageUrl { get; set; }

    [MaxLength(100)]
    [DisplayName("Stock Keeping Unit")]
    public string? Sku { get; set; }// Stock Keeping Unit

    [Range(0, 10000)]
    public int Stock { get; set; }

    [DefaultValue(false)]
    [DisplayName("Is Featured")]
    public bool IsFeatured { get; set; }

    [DefaultValue(false)]
    [DisplayName("On Sale")]
    public bool OnSale { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    private ICollection<Review>? _reviews;
    public ICollection<Review> Reviews 
    {
        get => _reviews ??= new List<Review>(); 
        set => _reviews = value;
    }
}
