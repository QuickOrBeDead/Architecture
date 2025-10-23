using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Api.Entities;
using RestaurantManagement.Api.IntegrationTests.Infrastructure;

namespace RestaurantManagement.Api.IntegrationTests.Data;

/// <summary>
/// Integration tests for RestaurantDbContext
/// </summary>
[TestFixture]
public class RestaurantDbContextTests : IntegrationTestBase
{
    [Test]
    public async Task DbContext_ShouldCreateDatabaseSuccessfully()
    {
        // Act & Assert
        var canConnect = await DbContext.Database.CanConnectAsync();
        canConnect.Should().BeTrue();
    }

    [Test]
    public void DbContext_ShouldHaveCorrectTablesConfigured()
    {
        // Arrange & Act
        var tableNames = DbContext.Model.GetEntityTypes()
            .Select(t => t.GetTableName())
            .OrderBy(name => name)
            .ToList();

        // Assert
        tableNames.Should().Contain("Tables");
        tableNames.Should().Contain("MenuItems");
        tableNames.Should().Contain("Orders");
        tableNames.Should().Contain("OrderItems");
    }

    [Test]
    public async Task DbContext_ShouldHaveSeedDataForTables()
    {
        // Act
        var tables = await DbContext.Tables.ToListAsync();

        // Assert
        tables.Should().HaveCount(5);
        tables.Should().AllSatisfy(t =>
        {
            t.Id.Should().BeGreaterThan(0);
            t.TableNumber.Should().BeGreaterThan(0);
            t.Capacity.Should().BeGreaterThan(0);
            t.Status.Should().Be(TableStatus.Available);
        });
    }

    [Test]
    public async Task DbContext_ShouldHaveSeedDataForMenuItems()
    {
        // Act
        var menuItems = await DbContext.MenuItems.ToListAsync();

        // Assert
        menuItems.Should().HaveCount(8);
        menuItems.Should().AllSatisfy(item =>
        {
            item.Id.Should().BeGreaterThan(0);
            item.Name.Should().NotBeNullOrWhiteSpace();
            item.Category.Should().NotBeNullOrWhiteSpace();
            item.Price.Should().BeGreaterThan(0);
            item.IsAvailable.Should().BeTrue();
        });

        // Verify specific categories exist
        var categories = menuItems.Select(m => m.Category).Distinct().ToList();
        categories.Should().Contain("Pizza");
        categories.Should().Contain("Salad");
        categories.Should().Contain("Pasta");
        categories.Should().Contain("Main Course");
        categories.Should().Contain("Dessert");
        categories.Should().Contain("Beverage");
    }

    [Test]
    public async Task Table_ShouldHaveCorrectConstraints()
    {
        // Arrange
        var table = new Table
        {
            TableNumber = 99,
            Capacity = 4,
            Status = TableStatus.Available
        };

        // Act
        DbContext.Tables.Add(table);
        await DbContext.SaveChangesAsync();

        // Assert
        var savedTable = await DbContext.Tables.FindAsync(table.Id);
        savedTable.Should().NotBeNull();
        savedTable!.TableNumber.Should().Be(99);
        savedTable.Capacity.Should().Be(4);
        savedTable.Status.Should().Be(TableStatus.Available);
    }

    [Test]
    public async Task MenuItem_ShouldRespectMaxLengthConstraints()
    {
        // Arrange
        var menuItem = new MenuItem
        {
            Name = new string('A', 200), // Max length
            Description = new string('B', 1000), // Max length
            Category = new string('C', 100), // Max length
            Price = 15.99m,
            IsAvailable = true
        };

        // Act
        DbContext.MenuItems.Add(menuItem);
        await DbContext.SaveChangesAsync();

        // Assert
        var savedMenuItem = await DbContext.MenuItems.FindAsync(menuItem.Id);
        savedMenuItem.Should().NotBeNull();
        savedMenuItem!.Name.Length.Should().Be(200);
        savedMenuItem.Description!.Length.Should().Be(1000);
        savedMenuItem.Category.Length.Should().Be(100);
    }

    [Test]
    public async Task MenuItem_ShouldHaveCorrectPrecisionForPrice()
    {
        // Arrange
        var menuItem = new MenuItem
        {
            Name = "Test Item",
            Category = "Test",
            Price = 123.45m,
            IsAvailable = true
        };

        // Act
        DbContext.MenuItems.Add(menuItem);
        await DbContext.SaveChangesAsync();

        // Assert
        var savedMenuItem = await DbContext.MenuItems.FindAsync(menuItem.Id);
        savedMenuItem.Should().NotBeNull();
        savedMenuItem!.Price.Should().Be(123.45m);
    }

    [Test]
    public async Task Order_ShouldHaveForeignKeyConstraintWithTable()
    {
        // Arrange
        var table = DbContext.Tables.First();
        var order = new Order
        {
            OrderNumber = "TEST001",
            TableId = table.Id,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            TotalAmount = 25.50m
        };

        // Act
        DbContext.Orders.Add(order);
        await DbContext.SaveChangesAsync();

        // Assert
        var savedOrder = await DbContext.Orders.FindAsync(order.Id);
        savedOrder.Should().NotBeNull();
        savedOrder!.TableId.Should().Be(table.Id);
    }

    [Test]
    public async Task OrderItem_ShouldHaveCascadeDeleteWithOrder()
    {
        // Arrange
        var table = DbContext.Tables.First();
        var menuItem = DbContext.MenuItems.First();
        
        var order = new Order
        {
            OrderNumber = "TEST002",
            TableId = table.Id,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            TotalAmount = 15.99m
        };
        
        DbContext.Orders.Add(order);
        await DbContext.SaveChangesAsync();

        var orderItem = new OrderItem
        {
            OrderId = order.Id,
            MenuItemId = menuItem.Id,
            Quantity = 2,
            Price = menuItem.Price
        };

        DbContext.OrderItems.Add(orderItem);
        await DbContext.SaveChangesAsync();

        // Act - Delete the order
        DbContext.Orders.Remove(order);
        await DbContext.SaveChangesAsync();

        // Assert - OrderItem should be cascade deleted
        var deletedOrderItem = await DbContext.OrderItems.FindAsync(orderItem.Id);
        deletedOrderItem.Should().BeNull();
    }

    [Test]
    public async Task Order_ShouldIncludeOrderItemsInNavigation()
    {
        // Arrange
        var table = DbContext.Tables.First();
        var menuItem1 = DbContext.MenuItems.First();
        var menuItem2 = DbContext.MenuItems.Skip(1).First();

        var order = new Order
        {
            OrderNumber = "TEST003",
            TableId = table.Id,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            TotalAmount = 30.50m,
            OrderItems = new List<OrderItem>
            {
                new() { MenuItemId = menuItem1.Id, Quantity = 1, Price = menuItem1.Price },
                new() { MenuItemId = menuItem2.Id, Quantity = 2, Price = menuItem2.Price }
            }
        };

        // Act
        DbContext.Orders.Add(order);
        await DbContext.SaveChangesAsync();

        // Assert
        var savedOrder = await DbContext.Orders
            .Include(o => o.OrderItems)
            .FirstAsync(o => o.Id == order.Id);

        savedOrder.OrderItems.Should().HaveCount(2);
        savedOrder.OrderItems.Should().AllSatisfy(item =>
        {
            item.OrderId.Should().Be(order.Id);
            item.Quantity.Should().BeGreaterThan(0);
            item.Price.Should().BeGreaterThan(0);
        });
    }

    [Test]
    public async Task DbContext_ShouldHandleConcurrentOperations()
    {
        // Arrange
        var table = DbContext.Tables.First();
        var menuItem = DbContext.MenuItems.First();

        // Act - Create multiple orders concurrently
        var tasks = Enumerable.Range(1, 5).Select(async i =>
        {
            using var context = GetFreshDbContext();
            var order = new Order
            {
                OrderNumber = $"CONCURRENT{i:D3}",
                TableId = table.Id,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                TotalAmount = i * 10.00m
            };
            
            context.Orders.Add(order);
            await context.SaveChangesAsync();
            return order.Id;
        });

        var orderIds = await Task.WhenAll(tasks);

        // Assert
        orderIds.Should().HaveCount(5);
        orderIds.Should().OnlyHaveUniqueItems();

        var orders = await DbContext.Orders
            .Where(o => orderIds.Contains(o.Id))
            .ToListAsync();
            
        orders.Should().HaveCount(5);
    }

    [Test]
    public async Task OrderItem_ShouldHaveCorrectPrecisionForPrice()
    {
        // Arrange
        var table = DbContext.Tables.First();
        var menuItem = DbContext.MenuItems.First();
        
        var order = new Order
        {
            OrderNumber = "PRICE_TEST",
            TableId = table.Id,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            TotalAmount = 99.99m
        };
        
        DbContext.Orders.Add(order);
        await DbContext.SaveChangesAsync();

        var orderItem = new OrderItem
        {
            OrderId = order.Id,
            MenuItemId = menuItem.Id,
            Quantity = 1,
            Price = 99.99m,
            SpecialInstructions = "Extra spicy"
        };

        // Act
        DbContext.OrderItems.Add(orderItem);
        await DbContext.SaveChangesAsync();

        // Assert
        var savedOrderItem = await DbContext.OrderItems.FindAsync(orderItem.Id);
        savedOrderItem.Should().NotBeNull();
        savedOrderItem!.Price.Should().Be(99.99m);
        savedOrderItem.SpecialInstructions.Should().Be("Extra spicy");
    }
}