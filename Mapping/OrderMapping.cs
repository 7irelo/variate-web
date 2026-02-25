using variate.Dtos;
using variate.Models;

namespace variate.Mapping
{
    public static class OrderMapping
        
    {
        public static OrderDto ToDto(this Order order) => new OrderDto
        {
            Id = order.Id,
            ApplicationUserId = order.ApplicationUserId,
            OrderDateTime = order.OrderDateTime,
            TotalCost = order.TotalCost,
            Status = order.Status,
            OrderItems = order.OrderItems.Select(oi => oi.ToDto()).ToList()
        };

        public static OrderItemDto ToDto(this OrderItem orderItem) => new OrderItemDto
        {
            Id = orderItem.Id,
            ProductId = orderItem.ProductId,
            Quantity = orderItem.Quantity,
            UnitPrice = orderItem.UnitPrice
        };

        public static Order ToEntity(this OrderDto orderDto) => new Order
        {
            Id = orderDto.Id,
            ApplicationUserId = orderDto.ApplicationUserId,
            OrderDateTime = orderDto.OrderDateTime,
            Status = orderDto.Status,
            OrderItems = orderDto.OrderItems.Select(oi => oi.ToEntity()).ToList()
        };

        public static OrderItem ToEntity(this OrderItemDto orderItemDto) => new OrderItem
        {
            Id = orderItemDto.Id,
            ProductId = orderItemDto.ProductId,
            Quantity = orderItemDto.Quantity,
            UnitPrice = orderItemDto.UnitPrice
        };
    }
}