namespace RestaurantManagement.Api.Features.Orders.UpdateOrderStatus;

using Mediator;

using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Api.Common;
using RestaurantManagement.Api.Data;
using RestaurantManagement.Api.Entities;

public sealed record UpdateOrderStatusCommand(int OrderId, OrderStatus NewStatus) : IRequest<Result<UpdateOrderStatusResponse>>;

public sealed class UpdateOrderStatusHandler(RestaurantDbContext context)
    : IRequestHandler<UpdateOrderStatusCommand, Result<UpdateOrderStatusResponse>>
{
    public async ValueTask<Result<UpdateOrderStatusResponse>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await context.Orders
                        .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            return Result<UpdateOrderStatusResponse>.NotFound($"Order with ID {request.OrderId} not found");
        }

        // Validate status transition
        if (!IsValidStatusTransition(order.Status, request.NewStatus))
        {
            return Result<UpdateOrderStatusResponse>.Failure($"Invalid status transition from {order.Status} to {request.NewStatus}");
        }

        order.Status = request.NewStatus;
        await context.SaveChangesAsync(cancellationToken);

        return Result<UpdateOrderStatusResponse>.Success(new UpdateOrderStatusResponse(order.Id, order.OrderNumber, order.Status.ToString()));
    }

    private static bool IsValidStatusTransition(OrderStatus current, OrderStatus next)
    {
        return (current, next) switch
        {
            (OrderStatus.Pending, OrderStatus.Confirmed) => true,
            (OrderStatus.Confirmed, OrderStatus.Preparing) => true,
            (OrderStatus.Preparing, OrderStatus.Ready) => true,
            (OrderStatus.Ready, OrderStatus.Served) => true,
            (OrderStatus.Served, OrderStatus.Completed) => true,
            (_, OrderStatus.Cancelled) => current != OrderStatus.Completed,
            _ => false
        };
    }
}