using System.ComponentModel.DataAnnotations;
namespace Variate.Models;

public class Category
{
    [Key] public int Id { get; set; }
    [Required] public required string Name { get; set; }
}
