namespace RestaurantManagement.Api.Features.Orders.GetKitchenOrders;

using Mediator;

using Microsoft.EntityFrameworkCore;

using RestaurantManagement.Api.Data;
using RestaurantManagement.Api.Entities;

public sealed record GetKitchenOrdersQuery : IRequest<GetKitchenOrdersResponse>;

public sealed class GetKitchenOrdersHandler(RestaurantDbContext context)
    : IRequestHandler<GetKitchenOrdersQuery, GetKitchenOrdersResponse>
{
    public async ValueTask<GetKitchenOrdersResponse> Handle(GetKitchenOrdersQuery request, CancellationToken cancellationToken)
    {
        var activeStatuses = new[] { OrderStatus.Confirmed, OrderStatus.Preparing, OrderStatus.Ready };

        var orders = await context.Orders
                         .Where(o => activeStatuses.Contains(o.Status))
                         .OrderBy(o => o.OrderDate)
                         .Select(o => new
                         {
                             Order = o,
                             TableNumber = context.Tables
                                                  .Where(t => t.Id == o.TableId)
                                                  .Select(t => t.TableNumber)
                                                  .FirstOrDefault(),
                             Items = context.OrderItems
                                                  .Where(oi => oi.OrderId == o.Id)
                                                  .Select(oi => new
                                                  {
                                                      OrderItem = oi,
                                                      MenuItem = context.MenuItems
                                                                            .FirstOrDefault(m => m.Id == oi.MenuItemId)
                                                  })
                                                  .ToList()
                         })
                         .ToListAsync(cancellationToken);

        var kitchenOrders = orders.Select(o => new KitchenOrderDto(
            o.Order.Id,
            o.Order.OrderNumber,
            o.TableNumber,
            o.Order.OrderDate,
            o.Order.Status.ToString(),
            [.. o.Items.Select(i => new KitchenOrderItemDto(
                i.MenuItem?.Name ?? "Unknown",
                i.MenuItem?.Category ?? "Unknown",
                i.OrderItem.Quantity,
                i.OrderItem.SpecialInstructions
            ))]
        )).ToList();

        return new GetKitchenOrdersResponse(kitchenOrders);
    }
}