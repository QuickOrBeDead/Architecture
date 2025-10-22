namespace RestaurantManagement.Api.Entities;

public class MenuItem
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Category { get; set; } = null!;

    public decimal Price { get; set; }

    public string? Description { get; set; }

    public bool IsAvailable { get; set; } = true;
}