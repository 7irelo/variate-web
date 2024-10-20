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
    
    [Required]
    [StringLength(100)]
    public string? Name { get; set; }
    
    [Required]
    [StringLength(200)]
    public string? Description { get; set; }
    
    [Range(0, 10000)]
    public decimal Price { get; set; }
    
    public DateOnly Release { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string? ImageUrl { get; set; }
    
    [DefaultValue(false)]
    public bool OnSale { get; set; } = false;
}

