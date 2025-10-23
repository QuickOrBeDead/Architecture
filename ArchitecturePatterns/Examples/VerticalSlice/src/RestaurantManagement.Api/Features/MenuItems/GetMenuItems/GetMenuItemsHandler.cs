namespace RestaurantManagement.Api.Features.MenuItems.GetMenuItems;

using Mediator;

using Microsoft.EntityFrameworkCore;

using RestaurantManagement.Api.Common;
using RestaurantManagement.Api.Data;

public sealed record GetMenuItemsQuery(string? Category = null) : IRequest<Result<GetMenuItemsResponse>>;

public sealed class GetMenuItemsHandler(RestaurantDbContext context) : IRequestHandler<GetMenuItemsQuery, Result<GetMenuItemsResponse>>
{
    public async ValueTask<Result<GetMenuItemsResponse>> Handle(GetMenuItemsQuery request, CancellationToken cancellationToken)
    {
        var query = context.MenuItems.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(m => m.Category == request.Category);
        }

        var menuItems = await query
                            .Where(m => m.IsAvailable)
                            .OrderBy(m => m.Category)
                            .ThenBy(m => m.Name)
                            .Select(m => new MenuItemDto(
                                m.Id,
                                m.Name,
                                m.Category,
                                m.Price,
                                m.Description,
                                m.IsAvailable))
                            .ToListAsync(cancellationToken);

        return Result<GetMenuItemsResponse>.Success(new GetMenuItemsResponse(menuItems));
    }
}