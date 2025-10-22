namespace RestaurantManagement.Api.Features.Tables.UpdateTableStatus;

using Mediator;

using Microsoft.EntityFrameworkCore;

using RestaurantManagement.Api.Data;
using RestaurantManagement.Api.Entities;

public record UpdateTableStatusCommand(int TableId, TableStatus NewStatus) : IRequest<UpdateTableStatusResponse>;

public class UpdateTableStatusHandler(RestaurantDbContext context)
    : IRequestHandler<UpdateTableStatusCommand, UpdateTableStatusResponse>
{
    public async ValueTask<UpdateTableStatusResponse> Handle(UpdateTableStatusCommand request, CancellationToken cancellationToken)
    {
        var table = await context.Tables
                        .FirstOrDefaultAsync(t => t.Id == request.TableId, cancellationToken);

        if (table == null)
        {
            throw new KeyNotFoundException($"Table with ID {request.TableId} not found");
        }

        table.Status = request.NewStatus;

        table.ReservedAt = request.NewStatus switch
        {
            TableStatus.Reserved => DateTime.UtcNow,
            TableStatus.Available => null,
            _ => table.ReservedAt
        };

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateTableStatusResponse(table.Id, table.TableNumber, table.Status.ToString());
    }
}