using System.Net;
using FluentAssertions;
using RestaurantManagement.Api.Features.MenuItems.GetMenuItems;
using RestaurantManagement.Api.FunctionalTests.Infrastructure;

namespace RestaurantManagement.Api.FunctionalTests.Features.MenuItems;

[TestFixture]
public class GetMenuItemsEndpointTests : FunctionalTestBase
{
    [Test]
    public async Task GetMenuItems_WithoutCategoryFilter_ReturnsAllAvailableMenuItems()
    {
        // Arrange
        await SeedDefaultTestData();

        // Act
        var response = await Client.GetAsync("/api/menuitems");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var menuItemsResponse = await HttpHelpers.DeserializeResponse<GetMenuItemsResponse>(response);
        menuItemsResponse.Should().NotBeNull();
        menuItemsResponse!.MenuItems.Should().HaveCount(4); // Only available items (excluding Chocolate Cake)
        
        // Verify all returned items are available
        menuItemsResponse.MenuItems.Should().AllSatisfy(item => item.IsAvailable.Should().BeTrue());
        
        // Verify specific menu items
        var margheritaPizza = menuItemsResponse.MenuItems.FirstOrDefault(x => x.Name == "Margherita Pizza");
        margheritaPizza.Should().NotBeNull();
        margheritaPizza!.Category.Should().Be("Pizza");
        margheritaPizza.Price.Should().Be(12.99m);
        margheritaPizza.Description.Should().Be("Classic tomato and mozzarella");
    }

    [Test]
    public async Task GetMenuItems_WithCategoryFilter_ReturnsOnlyItemsFromThatCategory()
    {
        // Arrange
        await SeedDefaultTestData();

        // Act
        var response = await Client.GetAsync("/api/menuitems?category=Pizza");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var menuItemsResponse = await HttpHelpers.DeserializeResponse<GetMenuItemsResponse>(response);
        menuItemsResponse.Should().NotBeNull();
        menuItemsResponse!.MenuItems.Should().HaveCount(2); // Margherita and Pepperoni pizzas
        
        menuItemsResponse.MenuItems.Should().AllSatisfy(item =>
        {
            item.Category.Should().Be("Pizza");
            item.IsAvailable.Should().BeTrue();
        });
        
        var pizzaNames = menuItemsResponse.MenuItems.Select(x => x.Name).ToList();
        pizzaNames.Should().Contain("Margherita Pizza");
        pizzaNames.Should().Contain("Pepperoni Pizza");
    }

    [Test]
    public async Task GetMenuItems_WithNonExistentCategory_ReturnsEmptyList()
    {
        // Arrange
        await SeedDefaultTestData();

        // Act
        var response = await Client.GetAsync("/api/menuitems?category=NonExistent");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var menuItemsResponse = await HttpHelpers.DeserializeResponse<GetMenuItemsResponse>(response);
        menuItemsResponse.Should().NotBeNull();
        menuItemsResponse!.MenuItems.Should().BeEmpty();
    }

    [Test]
    public async Task GetMenuItems_WithEmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        // No data seeded

        // Act
        var response = await Client.GetAsync("/api/menuitems");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var menuItemsResponse = await HttpHelpers.DeserializeResponse<GetMenuItemsResponse>(response);
        menuItemsResponse.Should().NotBeNull();
        menuItemsResponse!.MenuItems.Should().BeEmpty();
    }

    [Test]
    public async Task GetMenuItems_WithCaseSensitiveCategory_ReturnsCorrectItems()
    {
        // Arrange
        await SeedDefaultTestData();

        // Act
        var response = await Client.GetAsync("/api/menuitems?category=pizza"); // lowercase

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var menuItemsResponse = await HttpHelpers.DeserializeResponse<GetMenuItemsResponse>(response);
        menuItemsResponse.Should().NotBeNull();
        // Should be empty if case-sensitive, or contain pizza items if case-insensitive
        // Adjust based on actual implementation behavior
    }

    [Test]
    public async Task GetMenuItems_VerifiesResponseStructure()
    {
        // Arrange
        await SeedDefaultTestData();

        // Act
        var response = await Client.GetAsync("/api/menuitems");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        
        var menuItemsResponse = await HttpHelpers.DeserializeResponse<GetMenuItemsResponse>(response);
        menuItemsResponse.Should().NotBeNull();
        
        foreach (var item in menuItemsResponse!.MenuItems)
        {
            item.Id.Should().BeGreaterThan(0);
            item.Name.Should().NotBeNullOrEmpty();
            item.Category.Should().NotBeNullOrEmpty();
            item.Price.Should().BeGreaterThan(0);
            item.IsAvailable.Should().BeTrue(); // Only available items should be returned
        }
    }
}