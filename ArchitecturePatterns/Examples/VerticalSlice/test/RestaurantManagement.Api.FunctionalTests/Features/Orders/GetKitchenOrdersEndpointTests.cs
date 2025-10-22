using System.Net;
using FluentAssertions;
using RestaurantManagement.Api.Features.Orders.GetKitchenOrders;
using RestaurantManagement.Api.FunctionalTests.Infrastructure;

namespace RestaurantManagement.Api.FunctionalTests.Features.Orders;

[TestFixture]
public class GetKitchenOrdersEndpointTests : FunctionalTestBase
{
    [Test]
    public async Task GetKitchenOrders_ReturnsActiveOrders()
    {
        // Arrange
        await SeedDefaultTestData();

        // Act
        var response = await Client.GetAsync("/api/orders/kitchen");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var kitchenOrdersResponse = await HttpHelpers.DeserializeResponse<GetKitchenOrdersResponse>(response);
        kitchenOrdersResponse.Should().NotBeNull();
        kitchenOrdersResponse!.Orders.Should().HaveCount(2); // Two orders in test data
        
        // Verify order structure
        foreach (var order in kitchenOrdersResponse.Orders)
        {
            order.Id.Should().BeGreaterThan(0);
            order.OrderNumber.Should().NotBeNullOrEmpty();
            order.TableNumber.Should().BeGreaterThan(0);
            order.OrderDate.Should().BeBefore(DateTime.UtcNow);
            order.Status.Should().NotBeNullOrEmpty();
            order.Items.Should().NotBeEmpty();
        }
    }

    [Test]
    public async Task GetKitchenOrders_VerifiesOrderDetails()
    {
        // Arrange
        await SeedDefaultTestData();

        // Act
        var response = await Client.GetAsync("/api/orders/kitchen");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var kitchenOrdersResponse = await HttpHelpers.DeserializeResponse<GetKitchenOrdersResponse>(response);
        kitchenOrdersResponse.Should().NotBeNull();
        
        // Find the preparing order
        var preparingOrder = kitchenOrdersResponse!.Orders.FirstOrDefault(o => o.Status == "Preparing");
        preparingOrder.Should().NotBeNull();
        preparingOrder!.TableNumber.Should().Be(2);
        preparingOrder.OrderNumber.Should().Be("ORD-20231201-001");
        preparingOrder.Items.Should().HaveCount(3);
        
        // Verify menu item details in order items
        var pizzaItem = preparingOrder.Items.FirstOrDefault(i => i.MenuItemName == "Margherita Pizza");
        pizzaItem.Should().NotBeNull();
        pizzaItem!.Category.Should().Be("Pizza");
        pizzaItem.Quantity.Should().Be(1);
        pizzaItem.SpecialInstructions.Should().Be("Extra cheese");
    }

    [Test]
    public async Task GetKitchenOrders_VerifiesItemDetails()
    {
        // Arrange
        await SeedDefaultTestData();

        // Act
        var response = await Client.GetAsync("/api/orders/kitchen");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var kitchenOrdersResponse = await HttpHelpers.DeserializeResponse<GetKitchenOrdersResponse>(response);
        kitchenOrdersResponse.Should().NotBeNull();
        
        var allItems = kitchenOrdersResponse!.Orders.SelectMany(o => o.Items).ToList();
        
        foreach (var item in allItems)
        {
            item.MenuItemName.Should().NotBeNullOrEmpty();
            item.Category.Should().NotBeNullOrEmpty();
            item.Quantity.Should().BeGreaterThan(0);
        }
        
        // Check specific items exist
        var itemNames = allItems.Select(i => i.MenuItemName).ToList();
        itemNames.Should().Contain("Margherita Pizza");
        itemNames.Should().Contain("Caesar Salad");
        itemNames.Should().Contain("Grilled Chicken");
    }

    [Test]
    public async Task GetKitchenOrders_WithEmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        // No data seeded

        // Act
        var response = await Client.GetAsync("/api/orders/kitchen");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var kitchenOrdersResponse = await HttpHelpers.DeserializeResponse<GetKitchenOrdersResponse>(response);
        kitchenOrdersResponse.Should().NotBeNull();
        kitchenOrdersResponse!.Orders.Should().BeEmpty();
    }

    [Test]
    public async Task GetKitchenOrders_VerifiesResponseContentType()
    {
        // Arrange
        await SeedDefaultTestData();

        // Act
        var response = await Client.GetAsync("/api/orders/kitchen");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Test]
    public async Task GetKitchenOrders_OrdersShouldBeOrderedByOrderDate()
    {
        // Arrange
        await SeedDefaultTestData();

        // Act
        var response = await Client.GetAsync("/api/orders/kitchen");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var kitchenOrdersResponse = await HttpHelpers.DeserializeResponse<GetKitchenOrdersResponse>(response);
        kitchenOrdersResponse.Should().NotBeNull();
        
        if (kitchenOrdersResponse!.Orders.Count > 1)
        {
            // Orders should be ordered by date (implementation dependent)
            var orderDates = kitchenOrdersResponse.Orders.Select(o => o.OrderDate).ToList();
            // Check if orders are in chronological order or reverse chronological order
            var isAscending = orderDates.SequenceEqual(orderDates.OrderBy(d => d));
            var isDescending = orderDates.SequenceEqual(orderDates.OrderByDescending(d => d));
            
            (isAscending || isDescending).Should().BeTrue("Orders should be ordered by date");
        }
    }

    [Test]
    public async Task GetKitchenOrders_HandlesSpecialInstructionsCorrectly()
    {
        // Arrange
        await SeedDefaultTestData();

        // Act
        var response = await Client.GetAsync("/api/orders/kitchen");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var kitchenOrdersResponse = await HttpHelpers.DeserializeResponse<GetKitchenOrdersResponse>(response);
        kitchenOrdersResponse.Should().NotBeNull();
        
        var allItems = kitchenOrdersResponse!.Orders.SelectMany(o => o.Items).ToList();
        
        // Some items should have special instructions, some should be null
        var itemsWithInstructions = allItems.Where(i => !string.IsNullOrEmpty(i.SpecialInstructions)).ToList();
        var itemsWithoutInstructions = allItems.Where(i => string.IsNullOrEmpty(i.SpecialInstructions)).ToList();
        
        itemsWithInstructions.Should().NotBeEmpty("Some items should have special instructions");
        itemsWithoutInstructions.Should().NotBeEmpty("Some items should not have special instructions");
        
        // Verify specific special instruction
        var extraCheeseItem = allItems.FirstOrDefault(i => i.SpecialInstructions == "Extra cheese");
        extraCheeseItem.Should().NotBeNull();
        extraCheeseItem!.MenuItemName.Should().Be("Margherita Pizza");
    }
}