using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Variate.Models;

public class OrderItem
{
    [Key] public int Id { get; set; }
    [DisplayName("Order ID")]
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    [DisplayName("Unit Price")]
    public int UnitPrice { get; set; }
}
