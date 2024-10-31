using Microsoft.AspNetCore.Mvc;
using variate.Dtos;
using variate.Services;

namespace variate.Controllers
{
    [Route("orders")]
    [ApiController]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        // GET: orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> Index()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            if (!orders.Any())
            {
                _logger.LogWarning("No orders found.");
                return NotFound("No orders found.");
            }
            return View(orders);
        }

        // GET: orders/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderDto>> Details(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                _logger.LogWarning("Order with ID {OrderId} not found.", id);
                return NotFound($"Order with ID {id} not found.");
            }
            return View(order);
        }

        // POST: orders
        [HttpPost]
        public async Task<ActionResult<OrderDto>> Create(OrderDto orderDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for creating order.");
                return BadRequest(ModelState);
            }

            var createdOrder = await _orderService.CreateOrderAsync(orderDto);
            _logger.LogInformation("Order created successfully with ID {OrderId}.", createdOrder.Id);
            return CreatedAtAction(nameof(Details), new { id = createdOrder.Id }, createdOrder);
        }

        // PUT: orders/edit/{id}
        [HttpPut("edit/{id:int}")]
        public async Task<ActionResult<OrderDto>> Edit(int id, OrderDto orderDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for updating order with ID {OrderId}.", id);
                return BadRequest(ModelState);
            }

            var updatedOrder = await _orderService.UpdateOrderAsync(id, orderDto);
            if (updatedOrder == null)
            {
                return NotFound($"Order with ID {id} not found.");
            }

            _logger.LogInformation("Order with ID {OrderId} updated successfully.", id);
            return View(updatedOrder);
        }

        // DELETE: orders/delete/{id}
        [HttpDelete("delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _orderService.DeleteOrderAsync(id);
            if (!success)
            {
                _logger.LogWarning("Failed to delete order with ID {OrderId}.", id);
                return NotFound($"Order with ID {id} not found.");
            }

            _logger.LogInformation("Order with ID {OrderId} deleted successfully.", id);
            return RedirectToAction("Index");
        }
    }
}
