using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace variate.Models;

[Table("cart_items")]
public class CartItem : Product
{
    public int CartId { get; set; }
    
    public Cart? Cart { get; set; }

    public int Quantity { get; set; }

    [DisplayName("Unit Price")]
    public decimal UnitPrice { get; set; }

}
