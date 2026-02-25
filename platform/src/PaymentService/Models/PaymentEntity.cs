using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Variate.PaymentService.Models;

[Table("Payments")]
public class PaymentEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid OrderId { get; set; }

    [Required]
    [MaxLength(120)]
    public string UserId { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    [Required]
    [MaxLength(40)]
    public string Provider { get; set; } = "fake";

    [Required]
    [MaxLength(40)]
    public string Status { get; set; } = "Pending";

    [MaxLength(120)]
    public string? ExternalReference { get; set; }

    [MaxLength(500)]
    public string? FailureReason { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
