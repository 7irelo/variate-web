using variate.Models;
using variate.Dtos;

namespace variate.Mapping;

public static class ProductMapping
{
    public static Product ToEntity(this ProductDto productDto)
    {
        return new Product()
        {
            Id = productDto.Id,
            CategoryId = productDto.CategoryId,
            Category = productDto.Category,
            Name = productDto.Name,
            Description = productDto.Description,
            Colour = productDto.Colour,
            Size = productDto.Size,
            Price = productDto.Price,
            DiscountedPrice = productDto.DiscountedPrice,
            Release = productDto.Release,
            ImageUrl = productDto.ImageUrl,
            Sku = productDto.Sku,
            Stock = productDto.Stock,
            IsFeatured = productDto.IsFeatured,
            
        };
    }

    public static ProductDto ToDto(this Product product)
    {
        return new ProductDto(
            product.Id,
            product.CategoryId,
            product.Category,
            product.Brand,
            product.Name,
            product.Description,
            product.Colour,
            product.Size,
            product.Price,
            product.DiscountedPrice,
            product.Release,
            product.ImageUrl,
            product.Sku,
            product.Stock,
            product.IsFeatured,
            product.OnSale,
            product.CreatedAt,
            product.UpdatedAt
        );
    }
}
