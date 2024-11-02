using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using variate.Models;
using variate.Services;

namespace variate.Controllers
{
    [Authorize]
    [Route("cart")]
    public class CartController : Controller
    {
        private readonly ILogger<CartController> _logger;
        private readonly ICartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ILogger<CartController> logger, ICartService cartService, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _cartService = cartService;
            _userManager = userManager;
        }

        private IActionResult HandleError(Exception ex, string methodName, string message)
        {
            _logger.LogError(ex, "{Method}: {Message}", methodName, message);
            return RedirectToAction("Error", "Home");
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var userCart = await _cartService.GetOrCreateUserCartAsync(user.Id);
            return userCart == null ? RedirectToAction("Login", "Account") : View(userCart);
        }

        [HttpPost("create")]
        public async Task<IActionResult> AddToCart(int productId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var success = await _cartService.AddToCartAsync(user.Id, productId);
                return success ? RedirectToAction("Index") : BadRequest("Failed to add product to cart.");
            }
            catch (Exception e)
            {
                return HandleError(e, nameof(AddToCart), "Error adding product to cart.");
            }
        }

        [HttpPost("updateQuantity")]
        public async Task<IActionResult> UpdateQuantity(int productId, int delta)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var success = await _cartService.UpdateQuantityAsync(user.Id, productId, delta);
                return success ? Json(new { success = true }) : BadRequest("Failed to update product quantity.");
            }
            catch (Exception e)
            {
                return HandleError(e, nameof(UpdateQuantity), "Error updating quantity.");
            }
        }

        [HttpPost("delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var success = await _cartService.DeleteCartItemAsync(user.Id, id);
                return success ? RedirectToAction("Index") : NotFound("Item not found in cart.");
            }
            catch (Exception e)
            {
                return HandleError(e, nameof(Delete), $"Error deleting cart item with ID {id}.");
            }
        }
    }
}
