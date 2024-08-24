using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace variate.Models;

public class Review
{
    [Key] public int Id { get; set; }
    [DisplayName("Product ID")]
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    [DisplayName("User ID")]
    public int UserId { get; set; }
    public IdentityUser? IdentityUser { get; set; }
    public int Rating { get; set; }
    [DisplayName("Comment")]
    [Required][StringLength(200)]
    public string ReviewComment { get; set; }
}
