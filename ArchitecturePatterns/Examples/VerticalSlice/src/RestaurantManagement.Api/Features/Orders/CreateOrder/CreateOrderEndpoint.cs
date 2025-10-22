using Mediator;

using RestaurantManagement.Api.Common;

namespace RestaurantManagement.Api.Features.Orders.CreateOrder;

public static class CreateOrderEndpoint
{
    public static void MapCreateOrder(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders", async (
            IMediator mediator,
            CreateOrderRequest request) =>
        {
            var command = new CreateOrderCommand(request.TableId, request.Items, request.Notes);
            var result = await mediator.Send(command);

            return result.ToApiResult(data => Results.Created($"/api/orders/{data?.Id}", data));
        })
        .WithName("CreateOrder")
        .WithSummary("Create a new order")
        .WithOpenApi()
        .Accepts<CreateOrderRequest>("application/json")
        .Produces<CreateOrderResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}