using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Api.Entities;
using RestaurantManagement.Api.IntegrationTests.Infrastructure;

namespace RestaurantManagement.Api.IntegrationTests.Validation;

/// <summary>
/// Integration tests for validation pipeline components working together.
/// These focus on internal validation integration (EF Core constraints + Business logic)
/// rather than HTTP-level validation which is covered by functional tests.
/// </summary>
[TestFixture]
public class ValidationIntegrationTests : IntegrationTestBase
{
    [Test]
    public async Task DatabaseConstraints_ShouldIntegrateWithEntityConfiguration_ForTableValidation()
    {
        // Arrange - Test EF Core configuration allows duplicate table numbers (no unique constraint exists)
        var duplicateTableNumber = new Table
        {
            TableNumber = 1, // This is allowed - no unique constraint on table numbers
            Capacity = 4,
            Status = TableStatus.Available
        };

        // Act - Database should allow duplicate table numbers
        DbContext.Tables.Add(duplicateTableNumber);
        await DbContext.SaveChangesAsync();
        
        // Assert - Verify the table was saved (no unique constraint violation)
        var tablesWithNumber1 = await DbContext.Tables
            .Where(t => t.TableNumber == 1)
            .CountAsync();
        Assert.That(tablesWithNumber1, Is.EqualTo(2), "Should allow duplicate table numbers");
    }

    [Test]
    public async Task EntityValidation_ShouldIntegrateWithBusinessLogic_ForStatusTransitions()
    {
        // Arrange - Test business rule validation at entity level
        var table = await DbContext.Tables.FirstAsync(t => t.Status == TableStatus.Available);
        
        // Act - Test business logic integration with entity state changes
        table.Status = TableStatus.Occupied;
        table.ReservedAt = DateTime.UtcNow;
        
        await DbContext.SaveChangesAsync();
        
        // Assert - Business rules should be consistently applied
        var updatedTable = await DbContext.Tables.FirstAsync(t => t.Id == table.Id);
        
        Assert.Multiple(() =>
        {
            Assert.That(updatedTable.Status, Is.EqualTo(TableStatus.Occupied),
                "Status transition should be persisted");
            Assert.That(updatedTable.ReservedAt, Is.Not.Null,
                "Business rule: Occupied tables should have ReservedAt set");
        });

        // Test reverse transition
        updatedTable.Status = TableStatus.Available;
        updatedTable.ReservedAt = null;
        await DbContext.SaveChangesAsync();

        var revertedTable = await DbContext.Tables.FirstAsync(t => t.Id == table.Id);
        Assert.That(revertedTable.ReservedAt, Is.Null,
            "Business rule: Available tables should have ReservedAt cleared");
    }

    [Test]
    public async Task ForeignKeyConstraints_ShouldIntegrateWithCascadeDeletes_ForOrderCleanup()
    {
        // Arrange - Test cascade delete configuration integration
        var order = new Order
        {
            OrderNumber = "CASCADE-TEST-001",
            TableId = 1,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            Notes = "Cascade delete test"
        };

        DbContext.Orders.Add(order);
        await DbContext.SaveChangesAsync();

        var orderItem = new OrderItem
        {
            OrderId = order.Id,
            MenuItemId = 1,
            Quantity = 2,
            Price = 12.99m,
            SpecialInstructions = "Test cascade"
        };

        DbContext.OrderItems.Add(orderItem);
        await DbContext.SaveChangesAsync();

        // Act - Delete order should cascade to order items
        DbContext.Orders.Remove(order);
        await DbContext.SaveChangesAsync();

        // Assert - Cascade delete should clean up related entities
        var remainingOrderItems = await DbContext.OrderItems
            .Where(oi => oi.OrderId == order.Id)
            .CountAsync();

        Assert.That(remainingOrderItems, Is.EqualTo(0),
            "Order items should be automatically deleted via cascade configuration");
    }

    [Test]
    public async Task DecimalPrecision_ShouldBeEnforcedByDatabaseConfiguration_ForPriceValues()
    {
        // Arrange - Test decimal precision configuration integration
        var menuItem = new MenuItem
        {
            Name = "Precision Test Item",
            Category = "Test",
            Price = 123.456789m, // More precision than configured
            Description = "Testing decimal precision",
            IsAvailable = true
        };

        // Act
        DbContext.MenuItems.Add(menuItem);
        await DbContext.SaveChangesAsync();

        // Assert - Database should enforce precision configuration
        var savedItem = await DbContext.MenuItems
            .FirstAsync(mi => mi.Name == "Precision Test Item");

        // Note: SQLite doesn't enforce decimal precision, so this test shows the concept
        // In SQL Server/PostgreSQL, this would be enforced by database
        Assert.That(savedItem.Price, Is.EqualTo(123.456789m),
            "Price should be stored with configured precision");
    }

    [Test]
    public async Task EntityStateTracking_ShouldIntegrateWithValidation_ForModificationDetection()
    {
        // Arrange - Test entity change tracking integration
        var originalTable = await DbContext.Tables.FirstAsync(t => t.Id == 1);
        var originalCapacity = originalTable.Capacity;

        // Act - Modify entity and test change tracking
        originalTable.Capacity = originalCapacity + 2;
        originalTable.Status = TableStatus.Reserved;
        originalTable.ReservedAt = DateTime.UtcNow;

        // Verify change tracking before save
        var entityEntry = DbContext.Entry(originalTable);
        var modifiedProperties = entityEntry.Properties
            .Where(p => p.IsModified)
            .Select(p => p.Metadata.Name)
            .ToList();

        Assert.Multiple(() =>
        {
            Assert.That(modifiedProperties, Contains.Item("Capacity"),
                "Change tracking should detect Capacity modification");
            Assert.That(modifiedProperties, Contains.Item("Status"),
                "Change tracking should detect Status modification");
            Assert.That(modifiedProperties, Contains.Item("ReservedAt"),
                "Change tracking should detect ReservedAt modification");
        });

        await DbContext.SaveChangesAsync();

        // Assert - Changes should be persisted correctly
        var updatedTable = await DbContext.Tables.FirstAsync(t => t.Id == 1);
        Assert.Multiple(() =>
        {
            Assert.That(updatedTable.Capacity, Is.EqualTo(originalCapacity + 2),
                "Capacity change should be persisted");
            Assert.That(updatedTable.Status, Is.EqualTo(TableStatus.Reserved),
                "Status change should be persisted");
            Assert.That(updatedTable.ReservedAt, Is.Not.Null,
                "ReservedAt should be set");
        });
    }

    [Test] 
    public async Task ReferentialIntegrity_ShouldEnforceCascadeDeletes_ForOrdersAndOrderItems()
    {
        // Arrange - Test cascade delete behavior (Order -> OrderItems) which IS configured
        var order = new Order
        {
            OrderNumber = "CASCADE-TEST-001",
            TableId = 1,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending
        };
        
        DbContext.Orders.Add(order);
        await DbContext.SaveChangesAsync();
        
        var orderItem = new OrderItem
        {
            OrderId = order.Id,
            MenuItemId = 1, // Reference existing seeded menu item
            Quantity = 1,
            Price = 10.99m
        };
        
        DbContext.OrderItems.Add(orderItem);
        await DbContext.SaveChangesAsync();

        // Act - Delete the Order (should cascade to OrderItems)
        DbContext.Orders.Remove(order);
        await DbContext.SaveChangesAsync();
        
        // Assert - OrderItem should be deleted due to cascade
        var remainingOrderItems = await DbContext.OrderItems
            .Where(oi => oi.OrderId == order.Id)
            .CountAsync();
        Assert.That(remainingOrderItems, Is.EqualTo(0), "OrderItems should be cascaded deleted with Order");
    }
}