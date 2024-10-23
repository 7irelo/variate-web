using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace variate.Models;

[Table("order_items")]
public class OrderItem
{
    [Key]
    public int Id { get; set; }

    [DisplayName("Order ID")]
    public int OrderId { get; set; }

    public Order? Order { get; set; }

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    public int Quantity { get; set; }

    [DisplayName("Unit Price")]
    public decimal UnitPrice { get; set; }
}