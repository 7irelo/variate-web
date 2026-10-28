using variate.Dtos;
using variate.Models;

namespace variate.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        // A null userId means any customer's orders (admin view).
        Task<List<Order>> GetCustomerOrdersAsync(string? userId);
        Task<Order?> GetCustomerOrderAsync(int id, string? userId);
        Task<OrderDto?> GetOrderByIdAsync(int id);
        Task<OrderDto> CreateOrderAsync(OrderDto orderDto);
        Task<OrderDto?> UpdateOrderAsync(int id, OrderDto orderDto);
        Task<bool> DeleteOrderAsync(int id);
    }
}