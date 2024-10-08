using System.ComponentModel.DataAnnotations;

namespace variate.Dtos;

public record class CreateProductDto(
    [Required][StringLength(100)] string Name, 
    int CategoryId, 
    [Range(0, 10000)] decimal Price, 
    DateOnly Release);
