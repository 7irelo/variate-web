using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace variate.Models;

[Table("cart_items")]
public class CartItem
{
    [Key]
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int CartId { get; set; }
    public Cart? Cart { get; set; }
    public int Quantity { get; set; }
    [DisplayName("Unit Price")]
    public decimal UnitPrice { get; set; }

      // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
