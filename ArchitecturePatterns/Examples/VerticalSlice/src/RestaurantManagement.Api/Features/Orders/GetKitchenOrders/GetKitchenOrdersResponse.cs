namespace RestaurantManagement.Api.Features.Orders.GetKitchenOrders;

public record GetKitchenOrdersResponse(List<KitchenOrderDto> Orders);

public record KitchenOrderDto(
    int Id,
    string OrderNumber,
    int TableNumber,
    DateTime OrderDate,
    string Status,
    List<KitchenOrderItemDto> Items);

public record KitchenOrderItemDto(
    string MenuItemName,
    string Category,
    int Quantity,
    string? SpecialInstructions);