using FluentValidation;

using Mediator;

using Microsoft.EntityFrameworkCore;

using RestaurantManagement.Api.Common;
using RestaurantManagement.Api.Common.Behaviors;
using RestaurantManagement.Api.Data;
using RestaurantManagement.Api.Features.MenuItems.GetMenuItems;
using RestaurantManagement.Api.Features.Orders.CreateOrder;
using RestaurantManagement.Api.Features.Orders.GetKitchenOrders;
using RestaurantManagement.Api.Features.Orders.UpdateOrderStatus;
using RestaurantManagement.Api.Features.Tables.GetAllTables;
using RestaurantManagement.Api.Features.Tables.UpdateTableStatus;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Database context - using In-Memory for demo
builder.Services.AddDbContext<RestaurantDbContext>(options =>
    options.UseInMemoryDatabase("RestaurantDb"));

// Register FluentValidation validators
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly, ServiceLifetime.Singleton);

// Mediator - source generator based, high performance with pipeline behaviors
builder.Services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
});

// Register ValidationBehavior as open generic
builder.Services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Register ValidationBehavior for each discovered IRequest<Result<TResponse>> type
foreach (var type in typeof(Program).Assembly.GetTypes())
{
    foreach (var responseType in type.GetInterfaces()
                 .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))
                 .Select(requestInterface => requestInterface.GetGenericArguments()[0])
                 .Where(responseType => responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>)))
    {
        builder.Services.AddSingleton(
            typeof(IPipelineBehavior<,>).MakeGenericType(type, responseType), 
            typeof(ValidationBehavior<,>).MakeGenericType(type, responseType.GetGenericArguments()[0]));
    }
}

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();
    await context.Database.EnsureCreatedAsync().ConfigureAwait(false);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Map minimal API endpoints
app.MapGetMenuItems();
app.MapCreateOrder();
app.MapUpdateOrderStatus();
app.MapGetKitchenOrders();
app.MapGetAllTables();
app.MapUpdateTableStatus();

await app.RunAsync().ConfigureAwait(false);

// Make Program class accessible for testing
public partial class Program { }
