namespace RestaurantManagement.Api.Entities;

public class Table
{
    public int Id { get; set; }
    public int TableNumber { get; set; }
    public int Capacity { get; set; }
    public TableStatus Status { get; set; }
    public DateTime? ReservedAt { get; set; }
}