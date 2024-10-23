using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace variate.Models;

[Table("orders")]
public class Order
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string ApplicationUserId { get; set; } = string.Empty;

    public ApplicationUser? ApplicationUser { get; set; }

    public DateTime OrderDateTime { get; set; } = DateTime.Now;

    public decimal TotalCost => OrderItems.Sum(item => item.Quantity * item.UnitPrice);

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [Required, MaxLength(50)]
    public string? Status { get; set; }
}