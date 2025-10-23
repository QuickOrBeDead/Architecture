using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Api.Entities;
using RestaurantManagement.Api.Features.Orders.UpdateOrderStatus;
using RestaurantManagement.Api.FunctionalTests.Infrastructure;

namespace RestaurantManagement.Api.FunctionalTests.Features.Orders;

[TestFixture]
public class UpdateOrderStatusEndpointTests : FunctionalTestBase
{
    [Test]
    public async Task UpdateOrderStatus_WithValidData_UpdatesOrderSuccessfully()
    {
        // Arrange
        await SeedDefaultTestData();
        
        var request = new UpdateOrderStatusRequest(OrderStatus.Ready);

        // Act
        var response = await Client.PutAsync("/api/orders/1/status", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var orderResponse = await HttpHelpers.DeserializeResponse<UpdateOrderStatusResponse>(response);
        orderResponse.Should().NotBeNull();
        orderResponse!.Id.Should().Be(1);
        orderResponse.OrderNumber.Should().Be("ORD-20231201-001");
        orderResponse.Status.Should().Be("Ready");
        
        // Verify order was updated in database
        using var dbContext = GetDbContext();
        var updatedOrder = await dbContext.Orders.FindAsync(1);
        updatedOrder.Should().NotBeNull();
        updatedOrder!.Status.Should().Be(OrderStatus.Ready);
    }

    [Test]
    public async Task UpdateOrderStatus_WithNonExistentOrder_ReturnsNotFound()
    {
        // Arrange
        await SeedDefaultTestData();
        
        var request = new UpdateOrderStatusRequest(OrderStatus.Ready);

        // Act
        var response = await Client.PutAsync("/api/orders/999/status", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task UpdateOrderStatus_ToPending_UpdatesSuccessfully()
    {
        // Arrange - Create an order with Pending status to test valid transitions
        await SeedDatabase(context =>
        {
            context.Orders.Add(new Order
            {
                Id = 99,
                OrderNumber = "ORD-TEST-PENDING",
                TableId = 1,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending, // Start with pending
                TotalAmount = 15.99m,
                Notes = "Test order for pending status"
            });
        });
        
        var request = new UpdateOrderStatusRequest(OrderStatus.Confirmed); // Valid transition: Pending -> Confirmed

        // Act
        var response = await Client.PutAsync("/api/orders/99/status", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        using var dbContext = GetDbContext();
        var updatedOrder = await dbContext.Orders.FindAsync(99);
        updatedOrder!.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Test]
    public async Task UpdateOrderStatus_ToConfirmed_UpdatesSuccessfully()
    {
        // Arrange - Use order 2 which has Confirmed status, transition to Preparing
        await SeedDefaultTestData();
        
        var request = new UpdateOrderStatusRequest(OrderStatus.Preparing);

        // Act
        var response = await Client.PutAsync("/api/orders/2/status", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        using var dbContext = GetDbContext();
        var updatedOrder = await dbContext.Orders.FindAsync(2);
        updatedOrder!.Status.Should().Be(OrderStatus.Preparing);
    }

    [Test]
    public async Task UpdateOrderStatus_ToPreparing_UpdatesSuccessfully()
    {
        // Arrange
        await SeedDefaultTestData();
        
        var request = new UpdateOrderStatusRequest(OrderStatus.Preparing);

        // Act
        var response = await Client.PutAsync("/api/orders/2/status", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        using var dbContext = GetDbContext();
        var updatedOrder = await dbContext.Orders.FindAsync(2);
        updatedOrder!.Status.Should().Be(OrderStatus.Preparing);
    }

    [Test]
    public async Task UpdateOrderStatus_ToReady_UpdatesSuccessfully()
    {
        // Arrange
        await SeedDefaultTestData();
        
        var request = new UpdateOrderStatusRequest(OrderStatus.Ready);

        // Act
        var response = await Client.PutAsync("/api/orders/1/status", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        using var dbContext = GetDbContext();
        var updatedOrder = await dbContext.Orders.FindAsync(1);
        updatedOrder!.Status.Should().Be(OrderStatus.Ready);
    }

    [Test]
    public async Task UpdateOrderStatus_ToServed_UpdatesSuccessfully()
    {
        // Arrange - Create an order in Ready status to transition to Served
        await SeedDatabase(context =>
        {
            context.Orders.Add(new Order
            {
                Id = 98,
                OrderNumber = "ORD-TEST-READY",
                TableId = 1,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Ready,
                TotalAmount = 15.99m,
                Notes = "Test order for ready status"
            });
        });
        
        var request = new UpdateOrderStatusRequest(OrderStatus.Served);

        // Act
        var response = await Client.PutAsync("/api/orders/98/status", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        using var dbContext = GetDbContext();
        var updatedOrder = await dbContext.Orders.FindAsync(98);
        updatedOrder!.Status.Should().Be(OrderStatus.Served);
    }

    [Test]
    public async Task UpdateOrderStatus_ToCompleted_UpdatesSuccessfully()
    {
        // Arrange - Create an order in Served status to transition to Completed
        await SeedDatabase(context =>
        {
            context.Orders.Add(new Order
            {
                Id = 97,
                OrderNumber = "ORD-TEST-SERVED",
                TableId = 1,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Served,
                TotalAmount = 15.99m,
                Notes = "Test order for served status"
            });
        });
        
        var request = new UpdateOrderStatusRequest(OrderStatus.Completed);

        // Act
        var response = await Client.PutAsync("/api/orders/97/status", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        using var dbContext = GetDbContext();
        var updatedOrder = await dbContext.Orders.FindAsync(97);
        updatedOrder!.Status.Should().Be(OrderStatus.Completed);
    }

    [Test]
    public async Task UpdateOrderStatus_ToCancelled_UpdatesSuccessfully()
    {
        // Arrange
        await SeedDefaultTestData();
        
        var request = new UpdateOrderStatusRequest(OrderStatus.Cancelled);

        // Act
        var response = await Client.PutAsync("/api/orders/1/status", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        using var dbContext = GetDbContext();
        var updatedOrder = await dbContext.Orders.FindAsync(1);
        updatedOrder!.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Test]
    public async Task UpdateOrderStatus_WithInvalidOrderId_ReturnsNotFound()
    {
        // Arrange
        await SeedDefaultTestData();
        
        var request = new UpdateOrderStatusRequest(OrderStatus.Ready);

        // Act
        var response = await Client.PutAsync("/api/orders/999/status", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task UpdateOrderStatus_MultipleUpdates_AllSucceed()
    {
        // Arrange - Create an order in Pending status for proper state transitions
        await SeedDatabase(context =>
        {
            context.Orders.Add(new Order
            {
                Id = 96,
                OrderNumber = "ORD-TEST-MULTI",
                TableId = 1,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                TotalAmount = 25.99m,
                Notes = "Test order for multiple updates"
            });
        });

        // Act & Assert - Update order through different statuses
        var statuses = new[] { OrderStatus.Confirmed, OrderStatus.Preparing, OrderStatus.Ready, OrderStatus.Served };
        
        foreach (var status in statuses)
        {
            var request = new UpdateOrderStatusRequest(status);
            var response = await Client.PutAsync("/api/orders/96/status", HttpHelpers.ToJsonContent(request));
            
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var orderResponse = await HttpHelpers.DeserializeResponse<UpdateOrderStatusResponse>(response);
            orderResponse!.Status.Should().Be(status.ToString());
        }

        // Verify final status in database
        using var dbContext = GetDbContext();
        var finalOrder = await dbContext.Orders.FindAsync(96);
        finalOrder!.Status.Should().Be(OrderStatus.Served);
    }

    [Test]
    public async Task UpdateOrderStatus_VerifiesResponseStructure()
    {
        // Arrange
        await SeedDefaultTestData();
        
        var request = new UpdateOrderStatusRequest(OrderStatus.Ready);

        // Act
        var response = await Client.PutAsync("/api/orders/1/status", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        
        var orderResponse = await HttpHelpers.DeserializeResponse<UpdateOrderStatusResponse>(response);
        orderResponse.Should().NotBeNull();
        orderResponse!.Id.Should().BeGreaterThan(0);
        orderResponse.OrderNumber.Should().NotBeNullOrEmpty();
        orderResponse.Status.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task UpdateOrderStatus_DoesNotAffectOtherOrders()
    {
        // Arrange
        await SeedDefaultTestData();
        
        // Store original status of order 2
        using var originalDbContext = GetDbContext();
        var originalOrder2 = await originalDbContext.Orders.FindAsync(2);
        var originalStatus = originalOrder2!.Status;
        
        var request = new UpdateOrderStatusRequest(OrderStatus.Ready);

        // Act - Update order 1
        var response = await Client.PutAsync("/api/orders/1/status", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify order 2 wasn't affected
        using var dbContext = GetDbContext();
        var order2 = await dbContext.Orders.FindAsync(2);
        order2!.Status.Should().Be(originalStatus);
        
        // Verify order 1 was updated
        var order1 = await dbContext.Orders.FindAsync(1);
        order1!.Status.Should().Be(OrderStatus.Ready);
    }

    [TestCase(OrderStatus.Pending, OrderStatus.Preparing, TestName = "UpdateOrderStatus_InvalidTransition_PendingToPreparing_ReturnsBadRequest")]
    [TestCase(OrderStatus.Pending, OrderStatus.Ready, TestName = "UpdateOrderStatus_InvalidTransition_PendingToReady_ReturnsBadRequest")]
    [TestCase(OrderStatus.Pending, OrderStatus.Served, TestName = "UpdateOrderStatus_InvalidTransition_PendingToServed_ReturnsBadRequest")]
    [TestCase(OrderStatus.Pending, OrderStatus.Completed, TestName = "UpdateOrderStatus_InvalidTransition_PendingToCompleted_ReturnsBadRequest")]
    [TestCase(OrderStatus.Confirmed, OrderStatus.Pending, TestName = "UpdateOrderStatus_InvalidTransition_ConfirmedToPending_ReturnsBadRequest")]
    [TestCase(OrderStatus.Confirmed, OrderStatus.Ready, TestName = "UpdateOrderStatus_InvalidTransition_ConfirmedToReady_ReturnsBadRequest")]
    [TestCase(OrderStatus.Confirmed, OrderStatus.Served, TestName = "UpdateOrderStatus_InvalidTransition_ConfirmedToServed_ReturnsBadRequest")]
    [TestCase(OrderStatus.Confirmed, OrderStatus.Completed, TestName = "UpdateOrderStatus_InvalidTransition_ConfirmedToCompleted_ReturnsBadRequest")]
    [TestCase(OrderStatus.Preparing, OrderStatus.Pending, TestName = "UpdateOrderStatus_InvalidTransition_PreparingToPending_ReturnsBadRequest")]
    [TestCase(OrderStatus.Preparing, OrderStatus.Confirmed, TestName = "UpdateOrderStatus_InvalidTransition_PreparingToConfirmed_ReturnsBadRequest")]
    [TestCase(OrderStatus.Preparing, OrderStatus.Served, TestName = "UpdateOrderStatus_InvalidTransition_PreparingToServed_ReturnsBadRequest")]
    [TestCase(OrderStatus.Preparing, OrderStatus.Completed, TestName = "UpdateOrderStatus_InvalidTransition_PreparingToCompleted_ReturnsBadRequest")]
    [TestCase(OrderStatus.Ready, OrderStatus.Pending, TestName = "UpdateOrderStatus_InvalidTransition_ReadyToPending_ReturnsBadRequest")]
    [TestCase(OrderStatus.Ready, OrderStatus.Confirmed, TestName = "UpdateOrderStatus_InvalidTransition_ReadyToConfirmed_ReturnsBadRequest")]
    [TestCase(OrderStatus.Ready, OrderStatus.Preparing, TestName = "UpdateOrderStatus_InvalidTransition_ReadyToPreparing_ReturnsBadRequest")]
    [TestCase(OrderStatus.Ready, OrderStatus.Completed, TestName = "UpdateOrderStatus_InvalidTransition_ReadyToCompleted_ReturnsBadRequest")]
    [TestCase(OrderStatus.Served, OrderStatus.Pending, TestName = "UpdateOrderStatus_InvalidTransition_ServedToPending_ReturnsBadRequest")]
    [TestCase(OrderStatus.Served, OrderStatus.Confirmed, TestName = "UpdateOrderStatus_InvalidTransition_ServedToConfirmed_ReturnsBadRequest")]
    [TestCase(OrderStatus.Served, OrderStatus.Preparing, TestName = "UpdateOrderStatus_InvalidTransition_ServedToPreparing_ReturnsBadRequest")]
    [TestCase(OrderStatus.Served, OrderStatus.Ready, TestName = "UpdateOrderStatus_InvalidTransition_ServedToReady_ReturnsBadRequest")]
    [TestCase(OrderStatus.Completed, OrderStatus.Pending, TestName = "UpdateOrderStatus_InvalidTransition_CompletedToPending_ReturnsBadRequest")]
    [TestCase(OrderStatus.Completed, OrderStatus.Confirmed, TestName = "UpdateOrderStatus_InvalidTransition_CompletedToConfirmed_ReturnsBadRequest")]
    [TestCase(OrderStatus.Completed, OrderStatus.Preparing, TestName = "UpdateOrderStatus_InvalidTransition_CompletedToPreparing_ReturnsBadRequest")]
    [TestCase(OrderStatus.Completed, OrderStatus.Ready, TestName = "UpdateOrderStatus_InvalidTransition_CompletedToReady_ReturnsBadRequest")]
    [TestCase(OrderStatus.Completed, OrderStatus.Served, TestName = "UpdateOrderStatus_InvalidTransition_CompletedToServed_ReturnsBadRequest")]
    [TestCase(OrderStatus.Completed, OrderStatus.Cancelled, TestName = "UpdateOrderStatus_InvalidTransition_CompletedToCancelled_ReturnsBadRequest")]
    [TestCase(OrderStatus.Cancelled, OrderStatus.Pending, TestName = "UpdateOrderStatus_InvalidTransition_CancelledToPending_ReturnsBadRequest")]
    [TestCase(OrderStatus.Cancelled, OrderStatus.Confirmed, TestName = "UpdateOrderStatus_InvalidTransition_CancelledToConfirmed_ReturnsBadRequest")]
    [TestCase(OrderStatus.Cancelled, OrderStatus.Preparing, TestName = "UpdateOrderStatus_InvalidTransition_CancelledToPreparing_ReturnsBadRequest")]
    [TestCase(OrderStatus.Cancelled, OrderStatus.Ready, TestName = "UpdateOrderStatus_InvalidTransition_CancelledToReady_ReturnsBadRequest")]
    [TestCase(OrderStatus.Cancelled, OrderStatus.Served, TestName = "UpdateOrderStatus_InvalidTransition_CancelledToServed_ReturnsBadRequest")]
    [TestCase(OrderStatus.Cancelled, OrderStatus.Completed, TestName = "UpdateOrderStatus_InvalidTransition_CancelledToCompleted_ReturnsBadRequest")]
    public async Task UpdateOrderStatus_WithInvalidStatusTransition_ReturnsBadRequest(OrderStatus currentStatus, OrderStatus newStatus)
    {
        // Arrange - Create an order with the current status
        var orderId = 100 + (int)currentStatus; // Use different IDs to avoid conflicts
        await SeedDatabase(context =>
        {
            context.Orders.Add(new Order
            {
                Id = orderId,
                OrderNumber = $"ORD-TEST-{currentStatus}",
                TableId = 1,
                OrderDate = DateTime.UtcNow,
                Status = currentStatus,
                TotalAmount = 15.99m,
                Notes = $"Test order for invalid transition from {currentStatus} to {newStatus}"
            });
        });
        
        var request = new UpdateOrderStatusRequest(newStatus);

        // Act
        var response = await Client.PutAsync($"/api/orders/{orderId}/status", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        // Verify order status was not changed in database
        using var dbContext = GetDbContext();
        var unchangedOrder = await dbContext.Orders.FindAsync(orderId);
        unchangedOrder.Should().NotBeNull();
        unchangedOrder!.Status.Should().Be(currentStatus, "Order status should not change for invalid transitions");
    }

    [TestCase(OrderStatus.Pending, OrderStatus.Confirmed, TestName = "UpdateOrderStatus_ValidTransition_PendingToConfirmed_ReturnsOk")]
    [TestCase(OrderStatus.Confirmed, OrderStatus.Preparing, TestName = "UpdateOrderStatus_ValidTransition_ConfirmedToPreparing_ReturnsOk")]
    [TestCase(OrderStatus.Preparing, OrderStatus.Ready, TestName = "UpdateOrderStatus_ValidTransition_PreparingToReady_ReturnsOk")]
    [TestCase(OrderStatus.Ready, OrderStatus.Served, TestName = "UpdateOrderStatus_ValidTransition_ReadyToServed_ReturnsOk")]
    [TestCase(OrderStatus.Served, OrderStatus.Completed, TestName = "UpdateOrderStatus_ValidTransition_ServedToCompleted_ReturnsOk")]
    [TestCase(OrderStatus.Pending, OrderStatus.Cancelled, TestName = "UpdateOrderStatus_ValidTransition_PendingToCancelled_ReturnsOk")]
    [TestCase(OrderStatus.Confirmed, OrderStatus.Cancelled, TestName = "UpdateOrderStatus_ValidTransition_ConfirmedToCancelled_ReturnsOk")]
    [TestCase(OrderStatus.Preparing, OrderStatus.Cancelled, TestName = "UpdateOrderStatus_ValidTransition_PreparingToCancelled_ReturnsOk")]
    [TestCase(OrderStatus.Ready, OrderStatus.Cancelled, TestName = "UpdateOrderStatus_ValidTransition_ReadyToCancelled_ReturnsOk")]
    [TestCase(OrderStatus.Served, OrderStatus.Cancelled, TestName = "UpdateOrderStatus_ValidTransition_ServedToCancelled_ReturnsOk")]
    public async Task UpdateOrderStatus_WithValidStatusTransition_ReturnsOkAndUpdatesStatus(OrderStatus currentStatus, OrderStatus newStatus)
    {
        // Arrange - Create an order with the current status
        var orderId = 200 + (int)currentStatus; // Use different IDs to avoid conflicts with invalid transition tests
        await SeedDatabase(context =>
        {
            context.Orders.Add(new Order
            {
                Id = orderId,
                OrderNumber = $"ORD-VALID-{currentStatus}",
                TableId = 1,
                OrderDate = DateTime.UtcNow,
                Status = currentStatus,
                TotalAmount = 25.99m,
                Notes = $"Test order for valid transition from {currentStatus} to {newStatus}"
            });
        });
        
        var request = new UpdateOrderStatusRequest(newStatus);

        // Act
        var response = await Client.PutAsync($"/api/orders/{orderId}/status", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var orderResponse = await HttpHelpers.DeserializeResponse<UpdateOrderStatusResponse>(response);
        orderResponse.Should().NotBeNull();
        orderResponse!.Id.Should().Be(orderId);
        orderResponse.OrderNumber.Should().Be($"ORD-VALID-{currentStatus}");
        orderResponse.Status.Should().Be(newStatus.ToString());
        
        // Verify order status was updated in database
        using var dbContext = GetDbContext();
        var updatedOrder = await dbContext.Orders.FindAsync(orderId);
        updatedOrder.Should().NotBeNull();
        updatedOrder!.Status.Should().Be(newStatus, $"Order status should be updated from {currentStatus} to {newStatus}");
    }
}