using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using variate.Data;
using variate.Models;

namespace variate.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<CartService> _logger;

        public CartService(ApplicationDbContext db, ILogger<CartService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Cart> GetOrCreateUserCartAsync(string userId)
        {
            var cart = await _db.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId && c.Status == "Active");

            if (cart == null)
            {
                cart = new Cart
                {
                    ApplicationUserId = userId,
                    Status = "Active",
                    CreatedAt = DateTime.Now
                };
                _db.Carts.Add(cart);
                await _db.SaveChangesAsync();
            }

            return cart;
        }

        public async Task<bool> AddToCartAsync(string userId, int productId)
        {
            var userCart = await GetOrCreateUserCartAsync(userId);
            if (userCart == null) return false;

            var product = await _db.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null) return false;

            var existingCartItem = userCart.CartItems
                .FirstOrDefault(ci => ci.ProductId == productId);

            if (existingCartItem != null)
            {
                existingCartItem.Quantity += 1;
            }
            else
            {
                userCart.CartItems.Add(new CartItem
                {
                    ProductId = productId,
                    CartId = userCart.Id,
                    Quantity = 1,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateQuantityAsync(string userId, int productId, int delta)
        {
            var userCart = await GetOrCreateUserCartAsync(userId);
            if (userCart == null) return false;

            var cartItem = userCart.CartItems.FirstOrDefault(ci => ci.Id == productId);
            if (cartItem == null) return false;

            cartItem.Quantity = Math.Max(1, cartItem.Quantity + delta);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCartItemAsync(string userId, int productId)
        {
            var userCart = await GetOrCreateUserCartAsync(userId);
            if (userCart == null) return false;

            var cartItem = userCart.CartItems.FirstOrDefault(ci => ci.Id == productId);
            if (cartItem == null) return false;

            userCart.CartItems.Remove(cartItem);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
