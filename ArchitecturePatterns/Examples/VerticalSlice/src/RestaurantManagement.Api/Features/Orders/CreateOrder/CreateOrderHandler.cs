namespace RestaurantManagement.Api.Features.Orders.CreateOrder;

using Mediator;

using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Api.Common;
using RestaurantManagement.Api.Data;
using RestaurantManagement.Api.Entities;

public sealed record CreateOrderCommand(int TableId, List<OrderItemRequest> Items, string? Notes) : IRequest<Result<CreateOrderResponse>>;

public sealed class CreateOrderHandler(RestaurantDbContext context) : IRequestHandler<CreateOrderCommand, Result<CreateOrderResponse>>
{
    public async ValueTask<Result<CreateOrderResponse>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Validate table exists and is available
        var table = await context.Tables.FindAsync([request.TableId], cancellationToken);
        if (table == null)
        {
            return Result<CreateOrderResponse>.NotFound($"Table {request.TableId} not found");
        }

        if (table.Status is TableStatus.Occupied or TableStatus.Reserved)
        {
            return Result<CreateOrderResponse>.Failure($"Table {table.Id} is not available for orders");
        }

        // Get menu items
        var menuItemIds = request.Items.Select(i => i.MenuItemId).ToList();
        var menuItems = await context.MenuItems
                            .Where(m => menuItemIds.Contains(m.Id) && m.IsAvailable)
                            .ToListAsync(cancellationToken);

        // Check which menu items are not available
        var availableMenuItemIds = menuItems.Select(m => m.Id).ToList();
        var unavailableMenuItemIds = menuItemIds.Where(id => !availableMenuItemIds.Contains(id)).ToList();

        if (unavailableMenuItemIds.Count != 0)
        {
            return Result<CreateOrderResponse>.Failure(
                $"The following menu items are not available: {string.Join(", ", unavailableMenuItemIds)}",
                errorDetails: new Dictionary<string, object> { ["UnavailableMenuItemIds"] = unavailableMenuItemIds }
            );
        }

        // Create order items
        var orderItems = new List<OrderItem>();
        foreach (var itemRequest in request.Items)
        {
            var menuItem = menuItems.First(m => m.Id == itemRequest.MenuItemId);
            var orderItem = new OrderItem
            {
                MenuItemId = itemRequest.MenuItemId,
                Quantity = itemRequest.Quantity,
                Price = menuItem.Price,
                SpecialInstructions = itemRequest.SpecialInstructions
            };
            orderItems.Add(orderItem);
        }

        // Create order with order items in one shot
        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
        var order = new Order
        {
            OrderNumber = orderNumber,
            TableId = request.TableId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            Notes = request.Notes,
            TotalAmount = orderItems.Sum(i => i.Price * i.Quantity),
            OrderItems = orderItems
        };

        context.Orders.Add(order);
        await context.SaveChangesAsync(cancellationToken);

        // Return DTO
        var orderItemDtos = order.OrderItems.Select(oi =>
            {
                var menuItem = menuItems.First(m => m.Id == oi.MenuItemId);
                return new OrderItemDto(
                    oi.Id,
                    menuItem.Name,
                    oi.Quantity,
                    oi.Price,
                    oi.SpecialInstructions);
            }).ToList();

        var orderDto = new CreateOrderResponse(
            order.Id,
            order.OrderNumber,
            order.TableId,
            order.OrderDate,
            order.Status.ToString(),
            order.TotalAmount,
            orderItemDtos);

        return Result<CreateOrderResponse>.Success(orderDto);
    }
}