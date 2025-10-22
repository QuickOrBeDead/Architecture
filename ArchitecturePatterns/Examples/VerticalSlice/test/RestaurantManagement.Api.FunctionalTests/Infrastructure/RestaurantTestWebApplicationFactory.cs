using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestaurantManagement.Api.Data;

namespace RestaurantManagement.Api.FunctionalTests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory for functional testing with in-memory database
/// </summary>
public class RestaurantTestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<RestaurantDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Remove the RestaurantDbContext registration
            var contextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(RestaurantDbContext));
            if (contextDescriptor != null)
            {
                services.Remove(contextDescriptor);
            }

            // Add a test database context using a unique name for isolation
            var databaseName = $"RestaurantTestDb_{Guid.NewGuid()}";
            services.AddDbContext<RestaurantDbContext>(options =>
                options.UseInMemoryDatabase(databaseName));

            // Build the service provider and ensure database is created without seeding
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();
            
            // Ensure the database is created but clear any seeded data
            context.Database.EnsureCreated();
            
            // Clear all seeded data from the database
            context.Tables.RemoveRange(context.Tables);
            context.MenuItems.RemoveRange(context.MenuItems);
            context.Orders.RemoveRange(context.Orders);
            context.OrderItems.RemoveRange(context.OrderItems);
            context.SaveChanges();
        });

        builder.UseEnvironment("Testing");
    }
}