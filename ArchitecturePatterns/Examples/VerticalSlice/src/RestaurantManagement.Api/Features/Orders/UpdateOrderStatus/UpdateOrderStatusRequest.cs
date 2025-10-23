using RestaurantManagement.Api.Entities;

namespace RestaurantManagement.Api.Features.Orders.UpdateOrderStatus;

public record UpdateOrderStatusRequest(OrderStatus Status);