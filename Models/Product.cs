using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Variate.Models;

public class Product
{
    [Key] public int Id { get; set; }
    [DisplayName("Category ID")]
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    [Required][StringLength(20)] public string Name { get; set; }
    [Required][StringLength(200)] public string Description { get; set; }
    [Range(0, 10000)] public decimal Price { get; set; }
    public DateOnly Release { get; set; }
    public string ImageUrl { get; set; }
}
