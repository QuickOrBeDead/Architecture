namespace RestaurantManagement.Api.Features.MenuItems.GetMenuItems;

public record GetMenuItemsResponse(List<MenuItemDto> MenuItems);

public record MenuItemDto(
    int Id,
    string Name,
    string Category,
    decimal Price,
    string? Description,
    bool IsAvailable);