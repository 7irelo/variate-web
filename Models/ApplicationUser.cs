using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace variate.Models;

public class ApplicationUser : IdentityUser
{
    [Required, MaxLength(100)]
    public required string FirstName { get; set; }
    
    [Required, MaxLength(100)]
    public required string LastName { get; set; }
}