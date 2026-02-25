using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace variate.Models;

[Table("carts")]
public class Cart
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string ApplicationUserId { get; set; } = string.Empty;

    public ApplicationUser? ApplicationUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [DisplayName("Total Cost")]
    public decimal TotalCost => CartItems.Sum(item => item.Quantity * item.UnitPrice);

    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    [Required, MaxLength(50)]
    public string? Status { get; set; }
}