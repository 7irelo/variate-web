using System.ComponentModel.DataAnnotations;
namespace Variate.Models;

public class Product
{
    [key] public int Id { get; set; }
    public int CategoryId { get; set; }
    public Category? Category{ get; set; }
    [Required][StringLength(50)] public string Name { get; set; }
    [Range(0, 10000)] public decimal Price { get; set; }
    public DateOnly Release { get; set; } = DateOnly.Now;
}