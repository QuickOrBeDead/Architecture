namespace RestaurantManagement.Api.Features.Orders.CreateOrder;

public record CreateOrderResponse(
    int Id,
    string OrderNumber,
    int TableId,
    DateTime OrderDate,
    string Status,
    decimal TotalAmount,
    List<OrderItemDto> Items);

public record OrderItemDto(
    int Id,
    string MenuItemName,
    int Quantity,
    decimal Price,
    string? SpecialInstructions);