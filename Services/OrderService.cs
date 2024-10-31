using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using variate.Data;
using variate.Dtos;
using variate.Mapping;
using variate.Models;

namespace variate.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<OrderService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderService(ApplicationDbContext dbContext, ILogger<OrderService> logger, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            _logger.LogInformation("Retrieving all orders.");
            return await _dbContext.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Select(o => o.ToDto())
                .ToListAsync();
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving order with ID {OrderId}.", id);
            var order = await _dbContext.Orders
                .AsNoTracking()
                .Where(o => o.Id == id)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Select(o => o.ToDto())
                .FirstOrDefaultAsync();

            if (order == null)
            {
                _logger.LogWarning("Order with ID {OrderId} not found.", id);
            }

            return order;
        }

        public async Task<OrderDto> CreateOrderAsync(OrderDto orderDto)
        {
            var order = orderDto.ToEntity();
            await _dbContext.Orders.AddAsync(order);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Order with ID {OrderId} created successfully.", order.Id);

            return order.ToDto();
        }

        public async Task<OrderDto?> UpdateOrderAsync(int id, OrderDto orderDto)
        {
            if (id != orderDto.Id)
            {
                _logger.LogWarning("Order ID mismatch for update: route ID {RouteId}, DTO ID {DtoId}.", id, orderDto.Id);
                return null;
            }

            var existingOrder = await _dbContext.Orders.FindAsync(id);
            if (existingOrder == null)
            {
                _logger.LogWarning("Order with ID {OrderId} not found for update.", id);
                return null;
            }

            existingOrder = orderDto.ToEntity();
            _dbContext.Orders.Update(existingOrder);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Order with ID {OrderId} updated successfully.", id);

            return existingOrder.ToDto();
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await _dbContext.Orders.FindAsync(id);
            if (order == null)
            {
                _logger.LogWarning("Order with ID {OrderId} not found for deletion.", id);
                return false;
            }

            _dbContext.Orders.Remove(order);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Order with ID {OrderId} deleted successfully.", id);

            return true;
        }
    }
}
