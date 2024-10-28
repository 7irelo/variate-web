using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using variate.Data;
using variate.Models;
using System.Linq;
using System.Threading.Tasks;

namespace variate.Controllers
{
    [Authorize]
    [Route("cart")]
    public class CartController : Controller
    {
        private readonly ILogger<CartController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ILogger<CartController> logger, ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userCart = await GetOrCreateUserCart();

            if (userCart == null) return RedirectToAction("Login", "Account");

            return View(userCart);
        }

        [HttpPost("create")]
        public async Task<IActionResult> AddToCart(int productId, int quantity, string selectedColor, string selectedSize)
        {
            var userCart = await GetOrCreateUserCart();
            if (userCart == null) return BadRequest("User cart could not be found or created.");

            // Find the product and include the Category data
            var product = await _db.Products
                                .Include(p => p.Category)
                                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null) return NotFound("Product not found.");
            if (product.Category == null) return BadRequest("Product has an invalid CategoryId.");

            // Check if the item already exists in the cart
            var existingCartItem = userCart.CartItems
                                        .FirstOrDefault(ci => ci.Id == productId && ci.Colour == selectedColor && ci.Size == selectedSize);

            if (existingCartItem != null)
            {
                existingCartItem.Quantity += quantity;
            }
            else
            {
                var newCartItem = new CartItem
                {
                    CartId = userCart.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Brand = product.Brand,
                    Category = product.Category,
                    Description = product.Description,
                    ImageUrl = product.ImageUrl,
                    Colour = selectedColor,
                    Size = selectedSize,
                    Quantity = quantity
                };
                userCart.CartItems.Add(newCartItem);
            }

            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost("updateQuantity")]
        public async Task<IActionResult> UpdateQuantity(int productId, int delta)
        {
            var userCart = await GetOrCreateUserCart();
            if (userCart == null) return BadRequest("Cart not found.");

            var cartItem = userCart.CartItems.FirstOrDefault(ci => ci.Id == productId);
            if (cartItem == null) return NotFound("Product not found in cart.");

            cartItem.Quantity = Math.Max(1, cartItem.Quantity + delta);

            await _db.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userCart = await GetOrCreateUserCart();
            _logger.LogInformation("Cart Found");
            if (userCart == null) 
            {
                _logger.LogError("Cart with ID {Id} not found.", id);
                return BadRequest("Cart not found.");
            }

            var cartItem = userCart.CartItems.FirstOrDefault(ci => ci.Id == id);
            if (cartItem == null)
            {
                _logger.LogWarning("Cart Item with ID {Id} not found.", id);
                return NotFound();
            }

            try
            {
                userCart.CartItems.Remove(cartItem);
                _db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID {Id}.", id);
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the product.");
                return View(cartItem);
            }
        }

        // Method to get the current user's cart or create one if not found
        private async Task<Cart> GetOrCreateUserCart()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return null;

            var cart = await _db.Carts.Include(c => c.CartItems)
                                       .FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id && c.Status == "Active");

            if (cart == null)
            {
                cart = new Cart
                {
                    ApplicationUserId = user.Id,
                    Status = "Active",
                    CreatedAt = DateTime.Now
                };
                _db.Carts.Add(cart);
                await _db.SaveChangesAsync();
            }

            return cart;
        }
    }
}


