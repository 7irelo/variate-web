using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace variate.Models;

[Table("cart_items")]
public class CartItem
{
    [Key]
    public int Id { get; set; }
    
    [DisplayName("Cart ID")]
    public int CartId { get; set; }
    
    public Cart? Cart { get; set; }
    
    public int ProductId { get; set; }
    
    public Product? Product { get; set; }
    
    public int Quantity { get; set; }
    
    [DisplayName("Unit Price")]
    public int UnitPrice { get; set; }
}
