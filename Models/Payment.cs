using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Variate.Models;

public class Payment
{
    [Key] public int Id { get; set; }
    [DisplayName("Order ID")]
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public string PaymentMethod { get; set; }
    public string PaymentDate { get; set; }
    public decimal Amount { get; set; }
}
