using Mediator;
using RestaurantManagement.Api.Common;

namespace RestaurantManagement.Api.Features.Orders.UpdateOrderStatus;

public static class UpdateOrderStatusEndpoint
{
    public static void MapUpdateOrderStatus(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/orders/{orderId}/status", async (
            IMediator mediator,
            int orderId,
            UpdateOrderStatusRequest request) =>
        {
            var command = new UpdateOrderStatusCommand(orderId, request.Status);
            var result = await mediator.Send(command);
            return result.ToApiResult();
        })
        .WithName("UpdateOrderStatus")
        .WithSummary("Update order status")
        .WithOpenApi()
        .Accepts<UpdateOrderStatusRequest>("application/json")
        .Produces<UpdateOrderStatusResponse>();
    }
}