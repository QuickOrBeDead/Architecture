using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestaurantManagement.Api.Data;

namespace RestaurantManagement.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for integration tests that require database access
/// </summary>
public abstract class IntegrationTestBase
{
    protected RestaurantDbContext DbContext { get; private set; } = null!;
    protected IServiceProvider ServiceProvider { get; private set; } = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        
        // Add DbContext with SQLite database (using temp file for shared access)
        var connectionString = $"DataSource={Path.GetTempFileName()};Cache=Shared;";
        services.AddDbContext<RestaurantDbContext>(options =>
            options.UseSqlite(connectionString)
                   .EnableSensitiveDataLogging()
                   .EnableDetailedErrors());

        // Note: MediatR and other services can be added when testing specific features
        // For now, focus on database integration testing
        
        ServiceProvider = services.BuildServiceProvider();
    }

    [SetUp]
    public async Task SetUp()
    {
        // Create a new scope for each test
        var scope = ServiceProvider.CreateScope();
        DbContext = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();
        
        // Ensure database is created and migrated
        await DbContext.Database.EnsureCreatedAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        if (DbContext != null)
        {
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.DisposeAsync();
        }
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        if (ServiceProvider is IDisposable disposableProvider)
        {
            disposableProvider.Dispose();
        }
    }

    /// <summary>
    /// Gets a fresh instance of DbContext for testing scenarios that require multiple contexts
    /// </summary>
    protected RestaurantDbContext GetFreshDbContext()
    {
        var scope = ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();
        return context;
    }

    /// <summary>
    /// Seeds the database with test data
    /// </summary>
    protected async Task SeedTestDataAsync()
    {
        // The seed data is already included in the DbContext.OnModelCreating method
        // This method is for additional test-specific data if needed
        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Clears all data from the database while preserving schema
    /// </summary>
    protected async Task ClearDatabaseAsync()
    {
        DbContext.OrderItems.RemoveRange(DbContext.OrderItems);
        DbContext.Orders.RemoveRange(DbContext.Orders);
        DbContext.MenuItems.RemoveRange(DbContext.MenuItems);
        DbContext.Tables.RemoveRange(DbContext.Tables);
        await DbContext.SaveChangesAsync();
    }
}