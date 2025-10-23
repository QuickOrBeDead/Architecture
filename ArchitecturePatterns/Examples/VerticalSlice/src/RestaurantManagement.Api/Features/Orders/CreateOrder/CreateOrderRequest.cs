namespace RestaurantManagement.Api.Features.Orders.CreateOrder;

public record CreateOrderRequest(int TableId, List<OrderItemRequest> Items, string? Notes);

public record OrderItemRequest(int MenuItemId, int Quantity, string? SpecialInstructions);