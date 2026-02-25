using variate.Models;
using variate.Dtos;

namespace variate.Mapping
{
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
                Brand = productDto.Brand,
                Description = productDto.Description,
                Colour = productDto.Colour,
                Size = productDto.Size,
                Price = productDto.Price,
                DiscountedPrice = productDto.DiscountedPrice,
                ReleaseDate = productDto.ReleaseDate,
                ImageUrl = productDto.ImageUrl,
                Sku = productDto.Sku,
                Stock = productDto.Stock,
                IsFeatured = productDto.IsFeatured,
                Reviews = productDto.Reviews
            };
        }

        public static ProductDto ToDto(this Product product)
        {
            return new ProductDto(
                product.Id,
                product.CategoryId,
                product.Category ?? new Category {Name = "Unknown"},
                product.Brand,
                product.Name,
                product.Description ?? string.Empty,
                product.Colour ?? string.Empty,
                product.Size ?? string.Empty,
                product.Price,
                product.DiscountedPrice,
                product.ReleaseDate,
                product.ImageUrl ?? string.Empty,
                product.Sku ?? string.Empty,
                product.Stock,
                product.IsFeatured,
                product.OnSale,
                product.CreatedAt,
                product.UpdatedAt,
                product.Reviews
            );
        }
    }
}