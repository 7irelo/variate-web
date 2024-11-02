using System.Threading.Tasks;
using variate.Models;

namespace variate.Services
{
    public interface ICartService
    {
        Task<Cart> GetOrCreateUserCartAsync(string userId);
        Task<bool> AddToCartAsync(string userId, int productId);
        Task<bool> UpdateQuantityAsync(string userId, int productId, int delta);
        Task<bool> DeleteCartItemAsync(string userId, int productId);
    }
}