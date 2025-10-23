using Mediator;
using RestaurantManagement.Api.Common;

namespace RestaurantManagement.Api.Features.Tables.UpdateTableStatus;

public static class UpdateTableStatusEndpoint
{
    public static void MapUpdateTableStatus(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/tables/{tableId}/status", async (
            IMediator mediator,
            int tableId,
            UpdateTableStatusRequest request) =>
        {
            var command = new UpdateTableStatusCommand(tableId, request.Status);
            var result = await mediator.Send(command);
            return result.ToApiResult();
        })
        .WithName("UpdateTableStatus")
        .WithSummary("Update table status")
        .WithOpenApi()
        .Accepts<UpdateTableStatusRequest>("application/json")
        .Produces<UpdateTableStatusResponse>();
    }
}