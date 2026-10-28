using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using variate.Models;
using variate.Services;

namespace variate.Controllers
{
    // Customers see only their own orders; admins see everyone's.
    [Authorize]
    [Route("orders")]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(IOrderService orderService, UserManager<ApplicationUser> userManager)
        {
            _orderService = orderService;
            _userManager = userManager;
        }

        private string? ScopeUserId() => User.IsInRole("admin") ? null : _userManager.GetUserId(User);

        // GET: orders
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            return View(await _orderService.GetCustomerOrdersAsync(ScopeUserId()));
        }

        // GET: orders/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            // Another customer's order is a 404, not a 403, so ids can't be probed.
            var order = await _orderService.GetCustomerOrderAsync(id, ScopeUserId());
            return order == null ? NotFound() : View(order);
        }
    }
}
