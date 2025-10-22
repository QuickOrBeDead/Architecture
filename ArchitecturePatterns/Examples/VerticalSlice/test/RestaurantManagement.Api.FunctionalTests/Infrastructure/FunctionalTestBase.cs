using Microsoft.Extensions.DependencyInjection;
using RestaurantManagement.Api.Data;
using RestaurantManagement.Api.Entities;
using RestaurantManagement.Api.FunctionalTests.Infrastructure;

namespace RestaurantManagement.Api.FunctionalTests.Infrastructure;

/// <summary>
/// Base class for all functional tests providing common setup and utilities
/// </summary>
public abstract class FunctionalTestBase
{
    protected RestaurantTestWebApplicationFactory Factory { get; private set; } = null!;
    protected HttpClient Client { get; private set; } = null!;

    [SetUp]
    public virtual void Setup()
    {
        Factory = new RestaurantTestWebApplicationFactory();
        Client = Factory.CreateClient();
    }

    [TearDown]
    public virtual void TearDown()
    {
        Client?.Dispose();
        Factory?.Dispose();
    }

    /// <summary>
    /// Seeds the test database with sample data
    /// </summary>
    protected async Task SeedDatabase(Action<RestaurantDbContext> seedAction)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();
        
        seedAction(context);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Gets a fresh database context for assertions
    /// </summary>
    protected RestaurantDbContext GetDbContext()
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();
    }

    /// <summary>
    /// Seeds the database with default test data
    /// </summary>
    protected async Task SeedDefaultTestData()
    {
        await SeedDatabase(context =>
        {
            // Add test tables
            context.Tables.AddRange(
                new Table { Id = 1, TableNumber = 1, Capacity = 4, Status = TableStatus.Available },
                new Table { Id = 2, TableNumber = 2, Capacity = 6, Status = TableStatus.Occupied },
                new Table { Id = 3, TableNumber = 3, Capacity = 2, Status = TableStatus.Reserved, ReservedAt = DateTime.UtcNow }
            );

            // Add test menu items
            context.MenuItems.AddRange(
                new MenuItem { Id = 1, Name = "Margherita Pizza", Category = "Pizza", Price = 12.99m, Description = "Classic tomato and mozzarella", IsAvailable = true },
                new MenuItem { Id = 2, Name = "Caesar Salad", Category = "Salad", Price = 8.99m, Description = "Fresh romaine lettuce with caesar dressing", IsAvailable = true },
                new MenuItem { Id = 3, Name = "Grilled Chicken", Category = "Main Course", Price = 18.99m, Description = "Juicy grilled chicken breast", IsAvailable = true },
                new MenuItem { Id = 4, Name = "Chocolate Cake", Category = "Dessert", Price = 6.99m, Description = "Rich chocolate cake", IsAvailable = false },
                new MenuItem { Id = 5, Name = "Pepperoni Pizza", Category = "Pizza", Price = 14.99m, Description = "Classic pepperoni pizza", IsAvailable = true }
            );

            // Add test orders
            var order1 = new Order
            {
                Id = 1,
                OrderNumber = "ORD-20231201-001",
                TableId = 2,
                OrderDate = DateTime.UtcNow.AddMinutes(-30),
                Status = OrderStatus.Preparing,
                TotalAmount = 27.98m,
                Notes = "Test order 1"
            };

            var order2 = new Order
            {
                Id = 2,
                OrderNumber = "ORD-20231201-002",
                TableId = 1,
                OrderDate = DateTime.UtcNow.AddMinutes(-15),
                Status = OrderStatus.Confirmed,
                TotalAmount = 12.99m,
                Notes = "Test order 2"
            };

            context.Orders.AddRange(order1, order2);

            // Add order items
            context.OrderItems.AddRange(
                new OrderItem { Id = 1, OrderId = 1, MenuItemId = 1, Quantity = 1, Price = 12.99m, SpecialInstructions = "Extra cheese" },
                new OrderItem { Id = 2, OrderId = 1, MenuItemId = 2, Quantity = 1, Price = 8.99m, SpecialInstructions = null },
                new OrderItem { Id = 3, OrderId = 1, MenuItemId = 3, Quantity = 1, Price = 18.99m, SpecialInstructions = "Well done" },
                new OrderItem { Id = 4, OrderId = 2, MenuItemId = 1, Quantity = 1, Price = 12.99m, SpecialInstructions = null }
            );
        });
    }
}