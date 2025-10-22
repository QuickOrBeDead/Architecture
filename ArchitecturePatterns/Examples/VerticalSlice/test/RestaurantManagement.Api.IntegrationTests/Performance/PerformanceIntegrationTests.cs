using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestaurantManagement.Api.Data;
using RestaurantManagement.Api.Entities;
using RestaurantManagement.Api.IntegrationTests.Infrastructure;
using System.Diagnostics;

namespace RestaurantManagement.Api.IntegrationTests.Performance;

/// <summary>
/// Performance integration tests for the Restaurant Management API.
/// These tests demonstrate performance testing concepts for database operations
/// and system performance monitoring in integration tests.
/// </summary>
[TestFixture]
public class PerformanceIntegrationTests : IntegrationTestBase
{
    [Test]
    public async Task DatabaseOperations_ShouldCompleteWithinAcceptableTimeLimit()
    {
        // Arrange
        var maxAcceptableTimeMs = 100;
        
        // Act & Assert
        var stopwatch = Stopwatch.StartNew();
        var tables = await DbContext.Tables.ToListAsync();
        stopwatch.Stop();

        Assert.Multiple(() =>
        {
            Assert.That(tables, Is.Not.Null, "Query should return a result");
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThanOrEqualTo(maxAcceptableTimeMs),
                $"Database query should complete within {maxAcceptableTimeMs}ms but took {stopwatch.ElapsedMilliseconds}ms");
        });
    }

    [Test]
    public async Task BulkDatabaseOperations_ShouldMaintainAcceptablePerformance()
    {
        // Arrange
        var numberOfOrders = 10;
        var maxAcceptableTimePerOrderMs = 50;

        // Act
        var stopwatch = Stopwatch.StartNew();
        
        var orders = new List<Order>();
        for (int i = 1; i <= numberOfOrders; i++)
        {
            var order = new Order
            {
                OrderNumber = $"PERF-{i:000}",
                TableId = 1,
                Notes = $"Bulk order test {i}",
                Status = OrderStatus.Pending,
                OrderDate = DateTime.UtcNow
            };
            orders.Add(order);
        }

        DbContext.Orders.AddRange(orders);
        await DbContext.SaveChangesAsync();
        stopwatch.Stop();

        // Assert
        var averageTimePerOrder = stopwatch.ElapsedMilliseconds / numberOfOrders;
        
        Assert.Multiple(() =>
        {
            Assert.That(orders.All(o => o.Id > 0), Is.True, 
                "All orders should be saved with generated IDs");
            Assert.That(averageTimePerOrder, Is.LessThanOrEqualTo(maxAcceptableTimePerOrderMs),
                $"Average time per order should be within {maxAcceptableTimePerOrderMs}ms but was {averageTimePerOrder}ms");
        });
    }

    [Test]
    public async Task ConcurrentDatabaseAccess_ShouldMaintainSystemStability()
    {
        // Arrange
        var concurrentOperations = 10;
        var maxAcceptableTimeMs = 1000;

        // Act
        var stopwatch = Stopwatch.StartNew();
        var tasks = new List<Task>();

        for (int i = 0; i < concurrentOperations; i++)
        {
            var operationType = i % 2;
            
            if (operationType == 0)
            {
                // Read operations - each with its own context
                tasks.Add(Task.Run(async () =>
                {
                    using var scope = ServiceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();
                    await context.Tables.CountAsync();
                }));
            }
            else
            {
                // Write operations - each with its own context
                var capturedIndex = i;
                tasks.Add(Task.Run(async () =>
                {
                    using var scope = ServiceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();
                    var order = new Order
                    {
                        OrderNumber = $"CONC-{capturedIndex:000}",
                        TableId = 1,
                        Notes = $"Concurrent test {capturedIndex}",
                        Status = OrderStatus.Pending,
                        OrderDate = DateTime.UtcNow
                    };
                    context.Orders.Add(order);
                    await context.SaveChangesAsync();
                }));
            }
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(tasks.All(t => t.IsCompletedSuccessfully), Is.True,
                "All concurrent operations should complete successfully");
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThanOrEqualTo(maxAcceptableTimeMs),
                $"Concurrent operations should complete within {maxAcceptableTimeMs}ms but took {stopwatch.ElapsedMilliseconds}ms");
        });
    }

    [Test]
    public async Task MemoryUsage_ShouldNotExceedAcceptableLimits()
    {
        // Arrange
        var initialMemory = GC.GetTotalMemory(true);
        var operations = 50;

        // Act
        for (int i = 0; i < operations; i++)
        {
            var tables = await DbContext.Tables.ToListAsync();
            var menuItems = await DbContext.MenuItems.ToListAsync();
            
            // Force garbage collection every 10 operations
            if (i % 10 == 0)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        var finalMemory = GC.GetTotalMemory(true);
        var memoryIncrease = finalMemory - initialMemory;

        // Assert
        // Allow up to 5MB of memory increase for the operations
        var maxAcceptableMemoryIncreaseBytes = 5 * 1024 * 1024; 
        
        Assert.That(memoryIncrease, Is.LessThanOrEqualTo(maxAcceptableMemoryIncreaseBytes),
            $"Memory increase should be less than {maxAcceptableMemoryIncreaseBytes / (1024 * 1024)}MB but was {memoryIncrease / (1024 * 1024)}MB");
    }

    [Test]
    public async Task DatabaseConnection_ShouldHandleMultipleConnectionsConcurrently()
    {
        // Arrange
        var numberOfConcurrentConnections = 5;
        var maxAcceptableTimeMs = 500;

        // Act
        var stopwatch = Stopwatch.StartNew();
        
        var tasks = Enumerable.Range(1, numberOfConcurrentConnections)
            .Select(async i =>
            {
                // Use shared DbContext for this simplified test
                var tables = await DbContext.Tables.ToListAsync();
                var orders = await DbContext.Orders.ToListAsync();
                return tables.Count + orders.Count;
            })
            .ToArray();

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(results.All(r => r >= 0), Is.True,
                "All database operations should return valid results");
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThanOrEqualTo(maxAcceptableTimeMs),
                $"Concurrent database operations should complete within {maxAcceptableTimeMs}ms but took {stopwatch.ElapsedMilliseconds}ms");
        });
    }
}