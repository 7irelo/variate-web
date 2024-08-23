using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Variate.Models;

public class Order
{
    [Key] public int Id { get; set; }
    [DisplayName("User ID")]
    public int IdentityUserId { get; set; }
    public IdentityUser? IdentityUser { get; set; }
    [DisplayName("Order DateTime")]
    public DateTime OrderDateTime { get; set; } = DateTime.Now;
    [DisplayName("Total Cost")]
    public decimal TotalCost { get; set; }
    public string Status { get; set; }
}
