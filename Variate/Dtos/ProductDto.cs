namespace Variate.Dtos;

public record class ProductDto(int Id, string Name, string Genre, decimal Price, DateOnly Release);