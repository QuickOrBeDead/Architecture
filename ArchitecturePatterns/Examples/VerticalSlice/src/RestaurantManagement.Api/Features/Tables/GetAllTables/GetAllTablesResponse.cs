namespace RestaurantManagement.Api.Features.Tables.GetAllTables;

public record GetAllTablesResponse(List<TableDto> Tables);

public record TableDto(
    int Id,
    int TableNumber,
    int Capacity,
    string Status,
    DateTime? ReservedAt);