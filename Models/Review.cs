using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace variate.Models;

[Table("reviews")]
public class Review
{
    [Key]
    public int Id { get; set; }

    [Required]
    [DisplayName("Product ID")]
    public int ProductId { get; set; }
    
    public Product? Product { get; set; }

    [Required]
    [DisplayName("User ID")]
    public string ApplicationUserId { get; set; } = string.Empty;

    public ApplicationUser? ApplicationUser { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [Required, StringLength(200)]
    public string? ReviewComment { get; set; }

    public DateTime ReviewDate { get; set; } = DateTime.Now;
}
