namespace RestaurantManagement.Api.Features.MenuItems.GetMenuItems;

public record GetAllTablesResponse(List<TableDto> Tables);

public record TableDto(
    int Id,
    int TableNumber,
    int Capacity,
    string Status,
    DateTime? ReservedAt);