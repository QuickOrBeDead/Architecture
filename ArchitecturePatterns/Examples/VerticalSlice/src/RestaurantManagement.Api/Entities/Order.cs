namespace RestaurantManagement.Api.Entities;

public class Order
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = null!;

    public int TableId { get; set; }

    public DateTime OrderDate { get; set; }

    public OrderStatus Status { get; set; }

    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }

    // Navigation property for OrderItems
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}