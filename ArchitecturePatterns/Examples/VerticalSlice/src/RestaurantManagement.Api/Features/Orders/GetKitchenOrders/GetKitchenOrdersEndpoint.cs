using Mediator;

namespace RestaurantManagement.Api.Features.Orders.GetKitchenOrders;

public static class GetKitchenOrdersEndpoint
{
    public static void MapGetKitchenOrders(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/kitchen", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetKitchenOrdersQuery());
            return Results.Ok(result);
        })
        .WithName("GetKitchenOrders")
        .WithSummary("Get kitchen orders (for kitchen display)")
        .WithOpenApi()
        .Produces<GetKitchenOrdersResponse>();
    }
}