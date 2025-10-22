using Mediator;
using RestaurantManagement.Api.Features.MenuItems.GetMenuItems;

namespace RestaurantManagement.Api.Features.Tables.GetAllTables;

public static class GetAllTablesEndpoint
{
    public static void MapGetAllTables(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/tables", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetAllTablesQuery());
            return Results.Ok(result);
        })
        .WithName("GetAllTables")
        .WithSummary("Get all tables")
        .WithOpenApi()
        .Produces<GetAllTablesResponse>();
    }
}