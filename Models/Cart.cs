using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace variate.Models;

[Table("carts")]
public class Cart
{
    [Key]
    public int Id { get; set; }
    
    [DisplayName("User ID")]
    public string ApplicationUserId { get; set; } = string.Empty;
    
    public ApplicationUser? ApplicationUser { get; set; }

    [DisplayName("Added DateTime")]
    public DateTime AddedDateTime { get; set; } = DateTime.Now;

    [DisplayName("Total Cost")]
    public decimal TotalCost { get; set; }

    [Required, MaxLength(50)]
    public string? Status { get; set; }
}