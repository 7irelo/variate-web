using System.Collections.Generic;
using System.Threading.Tasks;
using variate.Dtos;
using variate.Models;

namespace variate.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync(string searchString);
        Task<Product> GetProductDetailsAsync(int id);
        Task<bool> CreateProductAsync(Product product);
        Task<Product> GetProductForEditAsync(int id);
        Task<bool> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(int id);
    }
}