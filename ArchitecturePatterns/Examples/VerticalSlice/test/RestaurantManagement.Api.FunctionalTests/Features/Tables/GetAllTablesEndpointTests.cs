using System.Net;
using FluentAssertions;
using RestaurantManagement.Api.Features.Tables.GetAllTables;
using RestaurantManagement.Api.FunctionalTests.Infrastructure;

namespace RestaurantManagement.Api.FunctionalTests.Features.Tables;

[TestFixture]
public class GetAllTablesEndpointTests : FunctionalTestBase
{
    [Test]
    public async Task GetAllTables_ReturnsAllTables()
    {
        // Arrange
        await SeedDefaultTestData();

        // Act
        var response = await Client.GetAsync("/api/tables");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var tablesResponse = await HttpHelpers.DeserializeResponse<GetAllTablesResponse>(response);
        tablesResponse.Should().NotBeNull();
        tablesResponse!.Tables.Should().HaveCount(3);
        
        // Verify tables are ordered by table number
        var tableNumbers = tablesResponse.Tables.Select(t => t.TableNumber).ToList();
        tableNumbers.Should().BeInAscendingOrder();
    }

    [Test]
    public async Task GetAllTables_VerifiesTableDetails()
    {
        // Arrange
        await SeedDefaultTestData();

        // Act
        var response = await Client.GetAsync("/api/tables");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var tablesResponse = await HttpHelpers.DeserializeResponse<GetAllTablesResponse>(response);
        tablesResponse.Should().NotBeNull();
        
        // Check available table
        var availableTable = tablesResponse!.Tables.FirstOrDefault(t => t.TableNumber == 1);
        availableTable.Should().NotBeNull();
        availableTable!.Id.Should().Be(1);
        availableTable.Capacity.Should().Be(4);
        availableTable.Status.Should().Be("Available");
        availableTable.ReservedAt.Should().BeNull();
        
        // Check occupied table
        var occupiedTable = tablesResponse.Tables.FirstOrDefault(t => t.TableNumber == 2);
        occupiedTable.Should().NotBeNull();
        occupiedTable!.Id.Should().Be(2);
        occupiedTable.Capacity.Should().Be(6);
        occupiedTable.Status.Should().Be("Occupied");
        occupiedTable.ReservedAt.Should().BeNull();
        
        // Check reserved table
        var reservedTable = tablesResponse.Tables.FirstOrDefault(t => t.TableNumber == 3);
        reservedTable.Should().NotBeNull();
        reservedTable!.Id.Should().Be(3);
        reservedTable.Capacity.Should().Be(2);
        reservedTable.Status.Should().Be("Reserved");
        reservedTable.ReservedAt.Should().NotBeNull();
        reservedTable.ReservedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
    }

    [Test]
    public async Task GetAllTables_WithEmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        // No data seeded

        // Act
        var response = await Client.GetAsync("/api/tables");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var tablesResponse = await HttpHelpers.DeserializeResponse<GetAllTablesResponse>(response);
        tablesResponse.Should().NotBeNull();
        tablesResponse!.Tables.Should().BeEmpty();
    }

    [Test]
    public async Task GetAllTables_VerifiesResponseStructure()
    {
        // Arrange
        await SeedDefaultTestData();

        // Act
        var response = await Client.GetAsync("/api/tables");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        
        var tablesResponse = await HttpHelpers.DeserializeResponse<GetAllTablesResponse>(response);
        tablesResponse.Should().NotBeNull();
        
        foreach (var table in tablesResponse!.Tables)
        {
            table.Id.Should().BeGreaterThan(0);
            table.TableNumber.Should().BeGreaterThan(0);
            table.Capacity.Should().BeGreaterThan(0);
            table.Status.Should().NotBeNullOrEmpty();
            table.Status.Should().BeOneOf("Available", "Occupied", "Reserved", "Cleaning");
        }
    }

    [Test]
    public async Task GetAllTables_VerifiesAllTableStatuses()
    {
        // Arrange
        await SeedDatabase(context =>
        {
            context.Tables.AddRange(
                new RestaurantManagement.Api.Entities.Table { Id = 1, TableNumber = 1, Capacity = 4, Status = RestaurantManagement.Api.Entities.TableStatus.Available },
                new RestaurantManagement.Api.Entities.Table { Id = 2, TableNumber = 2, Capacity = 6, Status = RestaurantManagement.Api.Entities.TableStatus.Occupied },
                new RestaurantManagement.Api.Entities.Table { Id = 3, TableNumber = 3, Capacity = 2, Status = RestaurantManagement.Api.Entities.TableStatus.Reserved, ReservedAt = DateTime.UtcNow },
                new RestaurantManagement.Api.Entities.Table { Id = 4, TableNumber = 4, Capacity = 8, Status = RestaurantManagement.Api.Entities.TableStatus.Cleaning }
            );
        });

        // Act
        var response = await Client.GetAsync("/api/tables");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var tablesResponse = await HttpHelpers.DeserializeResponse<GetAllTablesResponse>(response);
        tablesResponse.Should().NotBeNull();
        tablesResponse!.Tables.Should().HaveCount(4);
        
        var statuses = tablesResponse.Tables.Select(t => t.Status).ToList();
        statuses.Should().Contain("Available");
        statuses.Should().Contain("Occupied");
        statuses.Should().Contain("Reserved");
        statuses.Should().Contain("Cleaning");
    }

    [Test]
    public async Task GetAllTables_HandlesReservedAtFieldCorrectly()
    {
        // Arrange
        await SeedDefaultTestData();

        // Act
        var response = await Client.GetAsync("/api/tables");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var tablesResponse = await HttpHelpers.DeserializeResponse<GetAllTablesResponse>(response);
        tablesResponse.Should().NotBeNull();
        
        // Available and occupied tables should have null ReservedAt
        var nonReservedTables = tablesResponse!.Tables.Where(t => t.Status != "Reserved").ToList();
        nonReservedTables.Should().AllSatisfy(table => table.ReservedAt.Should().BeNull());
        
        // Reserved tables should have ReservedAt value
        var reservedTables = tablesResponse.Tables.Where(t => t.Status == "Reserved").ToList();
        reservedTables.Should().AllSatisfy(table => table.ReservedAt.Should().NotBeNull());
    }

    [Test]
    public async Task GetAllTables_OrdersByTableNumber()
    {
        // Arrange
        await SeedDatabase(context =>
        {
            context.Tables.AddRange(
                new RestaurantManagement.Api.Entities.Table { Id = 1, TableNumber = 5, Capacity = 4, Status = RestaurantManagement.Api.Entities.TableStatus.Available },
                new RestaurantManagement.Api.Entities.Table { Id = 2, TableNumber = 2, Capacity = 6, Status = RestaurantManagement.Api.Entities.TableStatus.Occupied },
                new RestaurantManagement.Api.Entities.Table { Id = 3, TableNumber = 8, Capacity = 2, Status = RestaurantManagement.Api.Entities.TableStatus.Reserved },
                new RestaurantManagement.Api.Entities.Table { Id = 4, TableNumber = 1, Capacity = 8, Status = RestaurantManagement.Api.Entities.TableStatus.Cleaning }
            );
        });

        // Act
        var response = await Client.GetAsync("/api/tables");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var tablesResponse = await HttpHelpers.DeserializeResponse<GetAllTablesResponse>(response);
        tablesResponse.Should().NotBeNull();
        
        var tableNumbers = tablesResponse!.Tables.Select(t => t.TableNumber).ToList();
        tableNumbers.Should().BeInAscendingOrder();
        tableNumbers.Should().Equal(1, 2, 5, 8);
    }
}