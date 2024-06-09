using System.ComponentModel.DataAnnotations;

namespace Variate.Dtos;

public record class UpdateProductDto(
    [Required][StringLength(50)] string Name, 
    [Required][StringLength(50)] string Category, 
    [Range(0, 2000)] decimal Price, 
    DateOnly Release);