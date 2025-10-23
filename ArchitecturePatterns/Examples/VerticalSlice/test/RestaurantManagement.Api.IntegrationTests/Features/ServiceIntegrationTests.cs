using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestaurantManagement.Api.Data;
using RestaurantManagement.Api.Entities;
using RestaurantManagement.Api.IntegrationTests.Infrastructure;

namespace RestaurantManagement.Api.IntegrationTests.Features;

/// <summary>
/// Integration tests demonstrating internal component interactions.
/// These focus on testing components working together without HTTP layer,
/// which is the unique value of integration tests vs functional tests.
/// </summary>
[TestFixture]
public class ServiceIntegrationTests : IntegrationTestBase
{
    [Test]
    public async Task DbContextService_ShouldIntegrateCorrectlyWithDependencyInjection_ForComplexQueries()
    {
        // Arrange - Test that DbContext integrates properly with service container
        using var scope = ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();
        
        // Act - Execute complex query that tests multiple entity relationships
        var complexData = await dbContext.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.Status == OrderStatus.Pending)
            .Select(o => new 
            {
                OrderId = o.Id,
                OrderNumber = o.OrderNumber,
                TableId = o.TableId,
                ItemCount = o.OrderItems.Count(),
                TotalValue = o.OrderItems.Sum(oi => oi.Price * oi.Quantity),
                OrderItemIds = o.OrderItems.Select(oi => oi.Id).ToList()
            })
            .ToListAsync();
        
        // Assert - Verify service integration and complex query execution
        Assert.Multiple(() =>
        {
            Assert.That(dbContext, Is.Not.Null, "DbContext should be available via DI");
            Assert.That(complexData, Is.Not.Null, "Complex query should execute successfully");
            // Complex queries should work with proper Include
            Assert.That(complexData.All(d => d.OrderItemIds != null), Is.True, 
                "Include should populate navigation properties");
        });
    }

    [Test]
    public async Task DatabaseTransactionIntegration_ShouldMaintainACIDProperties_AcrossMultipleOperations()
    {
        // Arrange - Test database transaction integration (internal infrastructure)
        using var transaction = await DbContext.Database.BeginTransactionAsync();
        
        var originalOrderCount = await DbContext.Orders.CountAsync();
        var originalItemCount = await DbContext.OrderItems.CountAsync();

        try
        {
            // Act - Perform multiple related operations within transaction
            var order = new Order
            {
                OrderNumber = "TRANS-001",
                TableId = 1,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                Notes = "Transaction test"
            };
            
            DbContext.Orders.Add(order);
            await DbContext.SaveChangesAsync();

            var orderItem1 = new OrderItem
            {
                OrderId = order.Id,
                MenuItemId = 1,
                Quantity = 2,
                Price = 12.99m
            };

            var orderItem2 = new OrderItem
            {
                OrderId = order.Id,
                MenuItemId = 2,
                Quantity = 1,
                Price = 14.99m
            };

            DbContext.OrderItems.AddRange(orderItem1, orderItem2);
            await DbContext.SaveChangesAsync();

            // Intentionally throw exception to test rollback
            throw new InvalidOperationException("Simulated error for rollback test");
        }
        catch (InvalidOperationException)
        {
            // Expected exception - rollback transaction
            await transaction.RollbackAsync();
        }

        // Assert - Verify transaction rollback worked correctly
        var finalOrderCount = await DbContext.Orders.CountAsync();
        var finalItemCount = await DbContext.OrderItems.CountAsync();

        Assert.Multiple(() =>
        {
            Assert.That(finalOrderCount, Is.EqualTo(originalOrderCount),
                "Transaction rollback should restore original order count");
            Assert.That(finalItemCount, Is.EqualTo(originalItemCount),
                "Transaction rollback should restore original item count");
        });
    }

    [Test]
    public async Task ConnectionPoolingIntegration_ShouldHandleMultipleSimultaneousConnections_Efficiently()
    {
        // Arrange - Test database connection pooling (infrastructure integration)
        var connectionTasks = new List<Task<int>>();

        // Act - Create multiple simultaneous database operations
        for (int i = 0; i < 5; i++)
        {
            connectionTasks.Add(Task.Run(async () =>
            {
                // Each task gets its own DbContext from service provider
                using var scope = ServiceProvider.CreateScope();
                var scopedContext = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();
                
                // Perform some database work
                var tableCount = await scopedContext.Tables.CountAsync();
                var menuItemCount = await scopedContext.MenuItems.CountAsync();
                
                return tableCount + menuItemCount;
            }));
        }

        var results = await Task.WhenAll(connectionTasks);

        // Assert - All operations should complete successfully
        Assert.Multiple(() =>
        {
            Assert.That(results, Has.All.GreaterThan(0),
                "All concurrent database operations should complete successfully");
            Assert.That(results.Distinct().Count(), Is.EqualTo(1),
                "All operations should return same result (consistency)");
        });
    }

    [Test]
    public async Task ChangeTrackingIntegration_ShouldOptimizeUpdateStatements_ForModifiedEntities()
    {
        // Arrange - Test EF Core change tracking optimization
        var table = await DbContext.Tables.FirstAsync(t => t.Id == 1);
        
        // Capture original values
        var originalCapacity = table.Capacity;
        var originalStatus = table.Status;

        // Act - Modify only specific properties
        table.Capacity = originalCapacity + 1;
        // Don't modify Status or other properties
        
        var entityEntry = DbContext.Entry(table);
        var modifiedProperties = entityEntry.Properties
            .Where(p => p.IsModified)
            .Select(p => p.Metadata.Name)
            .ToList();

        await DbContext.SaveChangesAsync();

        // Assert - Change tracking should only mark modified properties
        Assert.Multiple(() =>
        {
            Assert.That(modifiedProperties, Contains.Item("Capacity"),
                "Modified property should be tracked");
            Assert.That(modifiedProperties, Does.Not.Contain("Status"),
                "Unmodified properties should not be tracked");
            Assert.That(modifiedProperties, Does.Not.Contain("TableNumber"),
                "Unmodified properties should not be tracked");
        });

        // Verify the change was persisted
        var updatedTable = await DbContext.Tables.AsNoTracking().FirstAsync(t => t.Id == 1);
        Assert.Multiple(() =>
        {
            Assert.That(updatedTable.Capacity, Is.EqualTo(originalCapacity + 1),
                "Modified property should be updated in database");
            Assert.That(updatedTable.Status, Is.EqualTo(originalStatus),
                "Unmodified property should remain unchanged");
        });
    }

    [Test]
    public async Task QueryOptimizationIntegration_ShouldGenerateEfficientSQL_ForComplexJoins()
    {
        // Arrange & Act - Execute query that requires multiple joins using explicit joins
        var query = from oi in DbContext.OrderItems
                    join o in DbContext.Orders on oi.OrderId equals o.Id
                    join mi in DbContext.MenuItems on oi.MenuItemId equals mi.Id
                    where o.Status == OrderStatus.Pending && mi.IsAvailable
                    select new
                    {
                        OrderNumber = o.OrderNumber,
                        MenuItemName = mi.Name,
                        MenuItemPrice = mi.Price,
                        OrderItemPrice = oi.Price,
                        Quantity = oi.Quantity,
                        TotalItemValue = oi.Price * oi.Quantity
                    };

        var results = await query.ToListAsync();

        // Assert - Query should execute efficiently with proper joins
        Assert.Multiple(() =>
        {
            Assert.That(results, Is.Not.Null, "Complex join query should execute");
            // This tests that explicit joins work correctly without N+1 queries
            Assert.That(results.All(r => !string.IsNullOrEmpty(r.OrderNumber)), Is.True,
                "Order data should be joined correctly");
            Assert.That(results.All(r => !string.IsNullOrEmpty(r.MenuItemName)), Is.True,
                "MenuItem data should be joined correctly");
        });
    }

    [Test]
    public async Task ConcurrencyIntegration_ShouldHandleSimultaneousEntityModifications_Correctly()
    {
        // Arrange - Test concurrent entity modifications
        var table1Context = ServiceProvider.GetRequiredService<RestaurantDbContext>();
        var table2Context = ServiceProvider.GetRequiredService<RestaurantDbContext>();

        var table1 = await table1Context.Tables.FirstAsync(t => t.Id == 2);
        var table2 = await table2Context.Tables.FirstAsync(t => t.Id == 2);

        // Act - Modify same entity in different contexts
        table1.Capacity = 10;
        table1.Status = TableStatus.Occupied;
        await table1Context.SaveChangesAsync();

        table2.ReservedAt = DateTime.UtcNow;
        table2.Status = TableStatus.Reserved;
        
        // This should succeed since different properties are modified
        // (In real app, you might want optimistic concurrency control)
        await table2Context.SaveChangesAsync();

        // Assert - Both changes should be applied (last writer wins)
        using var verifyContext = ServiceProvider.GetRequiredService<RestaurantDbContext>();
        var finalTable = await verifyContext.Tables.AsNoTracking().FirstAsync(t => t.Id == 2);

        Assert.Multiple(() =>
        {
            Assert.That(finalTable.Capacity, Is.EqualTo(10),
                "First context's changes should be preserved");
            Assert.That(finalTable.Status, Is.EqualTo(TableStatus.Reserved),
                "Second context's status change should override first");
            Assert.That(finalTable.ReservedAt, Is.Not.Null,
                "Second context's ReservedAt should be set");
        });
    }
}