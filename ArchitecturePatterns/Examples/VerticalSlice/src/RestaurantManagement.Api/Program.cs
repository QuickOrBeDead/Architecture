using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Api.Common.Behaviors;
using RestaurantManagement.Api.Data;
using RestaurantManagement.Api.Features.MenuItems;
using RestaurantManagement.Api.Features.MenuItems.GetMenuItems;
using RestaurantManagement.Api.Features.Orders.CreateOrder;
using RestaurantManagement.Api.Features.Orders.GetKitchenOrders;
using RestaurantManagement.Api.Features.Orders.UpdateOrderStatus;
using RestaurantManagement.Api.Features.Tables.GetAllTables;
using RestaurantManagement.Api.Features.Tables.UpdateTableStatus;
using System.Reflection;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Database context - using In-Memory for demo
builder.Services.AddDbContext<RestaurantDbContext>(options =>
    options.UseInMemoryDatabase("RestaurantDb"));

// Register FluentValidation validators
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Mediator - source generator based, high performance with pipeline behaviors
builder.Services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
});

// Register pipeline behaviors for cross-cutting concerns
builder.Services.AddSingleton(typeof(Mediator.IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

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
