using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace variate.Models;

public class Order
{
    [Key] public int Id { get; set; }

    // Use string type to match the IdentityUser primary key type
    [DisplayName("User ID")]
    public string IdentityUserId { get; set; } = string.Empty;

    // This is the navigation property to the IdentityUser
    public IdentityUser? IdentityUser { get; set; }

    [DisplayName("Order DateTime")]
    public DateTime OrderDateTime { get; set; } = DateTime.Now;

    [DisplayName("Total Cost")]
    public decimal TotalCost { get; set; }

    public string? Status { get; set; }
}
