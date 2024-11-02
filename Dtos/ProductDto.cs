using variate.Models;

namespace variate.Dtos
{
    public record class ProductDto(
        int Id, 
        int CategoryId, 
        Category Category,
        string Brand,
        string Name,
        string Description,
        string Colour, 
        string Size, 
        decimal Price, 
        decimal DiscountedPrice, 
        DateOnly ReleaseDate, 
        string ImageUrl, 
        string Sku,
        int Stock,
        bool IsFeatured,
        bool OnSale,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        ICollection<Review> Reviews
    );
}
