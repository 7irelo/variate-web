using System.ComponentModel.DataAnnotations;
namespace Variate.Models;

public class Category
{
    [Key] public int Id { get; set; }
    public required string Name { get; set; }
}
