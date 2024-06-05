using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace variate.Models;

[Table("payments")]
public class Payment
{
    [Key]
    public int Id { get; set; }

    [Required]
    [DisplayName("Order ID")]
    public int OrderId { get; set; }

    public Order? Order { get; set; }

    [Required, MaxLength(50)]
    public string? PaymentMethod { get; set; }

    public DateTime PaymentDateTime { get; set; } = DateTime.Now;

    public decimal Amount { get; set; }

    [Required, MaxLength(50)]
    public string? Status { get; set; } // e.g., Pending, Completed, Failed
}
