
using Mediator;

using Microsoft.EntityFrameworkCore;

using RestaurantManagement.Api.Data;
using RestaurantManagement.Api.Features.MenuItems.GetMenuItems;

namespace RestaurantManagement.Api.Features.Tables.GetAllTables;

public sealed record GetAllTablesQuery : IRequest<GetAllTablesResponse>;

public sealed class GetAllTablesHandler(RestaurantDbContext context) : IRequestHandler<GetAllTablesQuery, GetAllTablesResponse>
{
    public async ValueTask<GetAllTablesResponse> Handle(GetAllTablesQuery request, CancellationToken cancellationToken)
    {
        var tables = await context.Tables
                         .OrderBy(t => t.TableNumber)
                         .Select(t => new TableDto(
                             t.Id,
                             t.TableNumber,
                             t.Capacity,
                             t.Status.ToString(),
                             t.ReservedAt))
                         .ToListAsync(cancellationToken);

        return new GetAllTablesResponse(tables);
    }
}