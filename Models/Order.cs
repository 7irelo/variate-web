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
    
    [DisplayName("User ID")]
    public string IdentityUserId { get; set; } = string.Empty;
    
    public IdentityUser? IdentityUser { get; set; }

    [DisplayName("Order DateTime")]
    public DateTime OrderDateTime { get; set; } = DateTime.Now;

    [DisplayName("Total Cost")]
    public decimal TotalCost { get; set; }

    [Required, MaxLength(50)]
    public string? Status { get; set; }
}
