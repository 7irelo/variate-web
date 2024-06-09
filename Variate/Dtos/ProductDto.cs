namespace Variate.Dtos;

public record class ProductDto(int Id, string Name, string Category, decimal Price, DateOnly Release);