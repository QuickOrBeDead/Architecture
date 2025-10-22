namespace RestaurantManagement.Api.Features.MenuItems.GetMenuItems;

using Mediator;

public static class GetMenuItemsEndpoint
{
    public static void MapGetMenuItems(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/menuitems", async (
            IMediator mediator,
            string? category = null) =>
        {
            var query = new GetMenuItemsQuery(category);
            var result = await mediator.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetMenuItems")
        .WithSummary("Get menu items, optionally filtered by category")
        .WithOpenApi()
        .Produces<GetMenuItemsResponse>();
    }
}