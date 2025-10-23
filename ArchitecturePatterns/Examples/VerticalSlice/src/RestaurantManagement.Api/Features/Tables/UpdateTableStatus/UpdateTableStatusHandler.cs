namespace RestaurantManagement.Api.Features.Tables.UpdateTableStatus;

using Mediator;

using Microsoft.EntityFrameworkCore;

using RestaurantManagement.Api.Common;
using RestaurantManagement.Api.Data;
using RestaurantManagement.Api.Entities;

public sealed record UpdateTableStatusCommand(int TableId, TableStatus NewStatus) : IRequest<Result<UpdateTableStatusResponse>>;

public sealed class UpdateTableStatusHandler(RestaurantDbContext context)
    : IRequestHandler<UpdateTableStatusCommand, Result<UpdateTableStatusResponse>>
{
    public async ValueTask<Result<UpdateTableStatusResponse>> Handle(UpdateTableStatusCommand request, CancellationToken cancellationToken)
    {
        var table = await context.Tables
                        .FirstOrDefaultAsync(t => t.Id == request.TableId, cancellationToken);

        if (table == null)
        {
            return Result<UpdateTableStatusResponse>.NotFound($"Table with ID {request.TableId} not found");
        }

        table.Status = request.NewStatus;

        table.ReservedAt = request.NewStatus switch
        {
            TableStatus.Reserved => DateTime.UtcNow,
            TableStatus.Available => null,
            _ => table.ReservedAt
        };

        await context.SaveChangesAsync(cancellationToken);

        return Result<UpdateTableStatusResponse>.Success(new UpdateTableStatusResponse(table.Id, table.TableNumber, table.Status.ToString()));
    }
}