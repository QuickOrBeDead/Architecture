namespace RestaurantManagement.Api.Features.Orders.UpdateOrderStatus;

public record UpdateOrderStatusResponse(int Id, string OrderNumber, string Status);