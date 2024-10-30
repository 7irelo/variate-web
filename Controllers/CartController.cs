using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using variate.Data;
using variate.Models;

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

        // Helper method for logging errors and redirecting
        private IActionResult HandleError(Exception ex, string methodName, string message)
        {
            _logger.LogError(ex, "{Method}: {Message}", methodName, message);
            return RedirectToAction("Error", "Home");
        }

        // GET: /cart
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var userCart = await GetOrCreateUserCartAsync();

            if (userCart == null) return RedirectToAction("Login", "Account", new { area = "Identity" });

            return View(userCart);
        }

        // POST: /cart/create
        [HttpPost("create")]
        public async Task<IActionResult> AddToCart(int productId, int quantity, string selectedColor, string selectedSize)
        {
            try
            {
                var userCart = await GetOrCreateUserCartAsync();
                if (userCart == null) return BadRequest("User cart could not be found or created.");

                var product = await _db.Products
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == productId);

                if (product == null) return NotFound("Product not found.");

                var existingCartItem = userCart.CartItems
                    .FirstOrDefault(ci => ci.Id == productId && ci.Colour == selectedColor && ci.Size == selectedSize);

                if (existingCartItem != null)
                {
                    existingCartItem.Quantity += quantity;
                }
                else
                {
                    userCart.CartItems.Add(new CartItem
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
                    });
                }

                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return HandleError(e, nameof(AddToCart), "Error adding product to cart.");
            }
        }

        // POST: /cart/updateQuantity
        [HttpPost("updateQuantity")]
        public async Task<IActionResult> UpdateQuantity(int productId, int delta)
        {
            try
            {
                var userCart = await GetOrCreateUserCartAsync();
                if (userCart == null) return BadRequest("Cart not found.");

                var cartItem = userCart.CartItems.FirstOrDefault(ci => ci.Id == productId);
                if (cartItem == null) return NotFound("Product not found in cart.");

                cartItem.Quantity = Math.Max(1, cartItem.Quantity + delta);
                await _db.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return HandleError(e, nameof(UpdateQuantity), "Error updating quantity.");
            }
        }

        // POST: /cart/delete/{id}
        [HttpPost("delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userCart = await GetOrCreateUserCartAsync();
                if (userCart == null)
                {
                    _logger.LogWarning("Cart not found for deletion.");
                    return BadRequest("Cart not found.");
                }

                var cartItem = userCart.CartItems.FirstOrDefault(ci => ci.Id == id);
                if (cartItem == null)
                {
                    _logger.LogWarning("Cart item with ID {Id} not found.", id);
                    return NotFound();
                }

                userCart.CartItems.Remove(cartItem);
                await _db.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return HandleError(e, nameof(Delete), $"Error deleting cart item with ID {id}.");
            }
        }

        // Method to get or create the user's cart
        private async Task<Cart> GetOrCreateUserCartAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return null;

            var cart = await _db.Carts
                .Include(c => c.CartItems)
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
