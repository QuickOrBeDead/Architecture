using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Api.Entities;
using RestaurantManagement.Api.Features.Orders.CreateOrder;
using RestaurantManagement.Api.FunctionalTests.Infrastructure;

namespace RestaurantManagement.Api.FunctionalTests.Features.Orders;

[TestFixture]
public class CreateOrderEndpointTests : FunctionalTestBase
{
    [Test]
    public async Task CreateOrder_WithValidData_ReturnsCreatedOrder()
    {
        // Arrange
        await SeedDefaultTestData();
        
        var request = new CreateOrderRequest(
            TableId: 1, // Available table
            Items: new List<OrderItemRequest>
            {
                new(MenuItemId: 1, Quantity: 2, SpecialInstructions: "Extra cheese"),
                new(MenuItemId: 2, Quantity: 1, SpecialInstructions: null)
            },
            Notes: "Birthday celebration"
        );

        // Act
        var response = await Client.PostAsync("/api/orders", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        
        var orderResponse = await HttpHelpers.DeserializeResponse<CreateOrderResponse>(response);
        orderResponse.Should().NotBeNull();
        orderResponse!.Id.Should().BeGreaterThan(0);
        orderResponse.OrderNumber.Should().StartWith("ORD-");
        orderResponse.TableId.Should().Be(1);
        orderResponse.Status.Should().Be("Pending");
        orderResponse.TotalAmount.Should().Be(34.97m); // (12.99 * 2) + 8.99
        orderResponse.Items.Should().HaveCount(2);
        
        // Verify order was saved to database
        using var dbContext = GetDbContext();
        var savedOrder = await dbContext.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderResponse.Id);
        
        savedOrder.Should().NotBeNull();
        savedOrder!.TableId.Should().Be(1);
        savedOrder.Status.Should().Be(OrderStatus.Pending);
        savedOrder.Notes.Should().Be("Birthday celebration");
        savedOrder.OrderItems.Should().HaveCount(2);
    }

    [Test]
    public async Task CreateOrder_WithNonExistentTable_ReturnsNotFound()
    {
        // Arrange
        await SeedDefaultTestData();
        
        var request = new CreateOrderRequest(
            TableId: 999, // Non-existent table
            Items: new List<OrderItemRequest>
            {
                new(MenuItemId: 1, Quantity: 1, SpecialInstructions: null)
            },
            Notes: null
        );

        // Act
        var response = await Client.PostAsync("/api/orders", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task CreateOrder_WithOccupiedTable_ReturnsBadRequest()
    {
        // Arrange
        await SeedDefaultTestData();
        
        var request = new CreateOrderRequest(
            TableId: 2, // Occupied table
            Items: new List<OrderItemRequest>
            {
                new(MenuItemId: 1, Quantity: 1, SpecialInstructions: null)
            },
            Notes: null
        );

        // Act
        var response = await Client.PostAsync("/api/orders", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task CreateOrder_WithReservedTable_ReturnsBadRequest()
    {
        // Arrange
        await SeedDefaultTestData();
        
        var request = new CreateOrderRequest(
            TableId: 3, // Reserved table
            Items: new List<OrderItemRequest>
            {
                new(MenuItemId: 1, Quantity: 1, SpecialInstructions: null)
            },
            Notes: null
        );

        // Act
        var response = await Client.PostAsync("/api/orders", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task CreateOrder_WithUnavailableMenuItem_ReturnsBadRequest()
    {
        // Arrange
        await SeedDefaultTestData();
        
        var request = new CreateOrderRequest(
            TableId: 1,
            Items: new List<OrderItemRequest>
            {
                new(MenuItemId: 4, Quantity: 1, SpecialInstructions: null) // Unavailable menu item
            },
            Notes: null
        );

        // Act
        var response = await Client.PostAsync("/api/orders", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task CreateOrder_WithNonExistentMenuItem_ReturnsBadRequest()
    {
        // Arrange
        await SeedDefaultTestData();
        
        var request = new CreateOrderRequest(
            TableId: 1,
            Items: new List<OrderItemRequest>
            {
                new(MenuItemId: 999, Quantity: 1, SpecialInstructions: null) // Non-existent menu item
            },
            Notes: null
        );

        // Act
        var response = await Client.PostAsync("/api/orders", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task CreateOrder_WithEmptyItems_ReturnsBadRequest()
    {
        // Arrange
        await SeedDefaultTestData();
        
        var request = new CreateOrderRequest(
            TableId: 1,
            Items: new List<OrderItemRequest>(), // Empty items list
            Notes: null
        );

        // Act
        var response = await Client.PostAsync("/api/orders", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task CreateOrder_WithInvalidTableId_ReturnsBadRequest()
    {
        // Arrange
        await SeedDefaultTestData();
        
        var request = new CreateOrderRequest(
            TableId: 0, // Invalid table ID
            Items: new List<OrderItemRequest>
            {
                new(MenuItemId: 1, Quantity: 1, SpecialInstructions: null)
            },
            Notes: null
        );

        // Act
        var response = await Client.PostAsync("/api/orders", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task CreateOrder_WithZeroQuantity_ReturnsBadRequest()
    {
        // Arrange
        await SeedDefaultTestData();
        
        var request = new CreateOrderRequest(
            TableId: 1,
            Items: new List<OrderItemRequest>
            {
                new(MenuItemId: 1, Quantity: 0, SpecialInstructions: null) // Zero quantity
            },
            Notes: null
        );

        // Act
        var response = await Client.PostAsync("/api/orders", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task CreateOrder_WithLongNotes_ReturnsBadRequest()
    {
        // Arrange
        await SeedDefaultTestData();
        
        var request = new CreateOrderRequest(
            TableId: 1,
            Items: new List<OrderItemRequest>
            {
                new(MenuItemId: 1, Quantity: 1, SpecialInstructions: null)
            },
            Notes: new string('A', 501) // Exceeds 500 character limit
        );

        // Act
        var response = await Client.PostAsync("/api/orders", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task CreateOrder_WithLongSpecialInstructions_ReturnsBadRequest()
    {
        // Arrange
        await SeedDefaultTestData();
        
        var request = new CreateOrderRequest(
            TableId: 1,
            Items: new List<OrderItemRequest>
            {
                new(MenuItemId: 1, Quantity: 1, SpecialInstructions: new string('A', 251)) // Exceeds 250 character limit
            },
            Notes: null
        );

        // Act
        var response = await Client.PostAsync("/api/orders", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task CreateOrder_VerifiesCalculatedTotalAmount()
    {
        // Arrange
        await SeedDefaultTestData();
        
        var request = new CreateOrderRequest(
            TableId: 1,
            Items: new List<OrderItemRequest>
            {
                new(MenuItemId: 1, Quantity: 3, SpecialInstructions: null), // 12.99 * 3 = 38.97
                new(MenuItemId: 3, Quantity: 2, SpecialInstructions: null)  // 18.99 * 2 = 37.98
            },
            Notes: null
        );

        // Act
        var response = await Client.PostAsync("/api/orders", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var orderResponse = await HttpHelpers.DeserializeResponse<CreateOrderResponse>(response);
        orderResponse.Should().NotBeNull();
        orderResponse!.TotalAmount.Should().Be(76.95m); // 38.97 + 37.98
    }
}