using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Api.Entities;
using RestaurantManagement.Api.Features.Tables.UpdateTableStatus;
using RestaurantManagement.Api.FunctionalTests.Infrastructure;

namespace RestaurantManagement.Api.FunctionalTests.Features.Tables;

[TestFixture]
public class UpdateTableStatusEndpointTests : FunctionalTestBase
{
    [Test]
    public async Task UpdateTableStatus_ToAvailable_UpdatesSuccessfully()
    {
        // Arrange
        await SeedDefaultTestData();
        
        var request = new UpdateTableStatusRequest(TableStatus.Available);

        // Act
        var response = await Client.PutAsync("/api/tables/2/status", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var tableResponse = await HttpHelpers.DeserializeResponse<UpdateTableStatusResponse>(response);
        tableResponse.Should().NotBeNull();
        tableResponse!.Id.Should().Be(2);
        tableResponse.TableNumber.Should().Be(2);
        tableResponse.Status.Should().Be("Available");
        
        // Verify table was updated in database
        using var dbContext = GetDbContext();
        var updatedTable = await dbContext.Tables.FindAsync(2);
        updatedTable.Should().NotBeNull();
        updatedTable!.Status.Should().Be(TableStatus.Available);
        updatedTable.ReservedAt.Should().BeNull(); // Should be cleared when set to Available
    }

    [Test]
    public async Task UpdateTableStatus_ToOccupied_UpdatesSuccessfully()
    {
        // Arrange
        await SeedDefaultTestData();
        
        var request = new UpdateTableStatusRequest(TableStatus.Occupied);

        // Act
        var response = await Client.PutAsync("/api/tables/1/status", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var tableResponse = await HttpHelpers.DeserializeResponse<UpdateTableStatusResponse>(response);
        tableResponse.Should().NotBeNull();
        tableResponse!.Id.Should().Be(1);
        tableResponse.TableNumber.Should().Be(1);
        tableResponse.Status.Should().Be("Occupied");
        
        // Verify table was updated in database
        using var dbContext = GetDbContext();
        var updatedTable = await dbContext.Tables.FindAsync(1);
        updatedTable.Should().NotBeNull();
        updatedTable!.Status.Should().Be(TableStatus.Occupied);
    }

    [Test]
    public async Task UpdateTableStatus_ToReserved_UpdatesSuccessfullyAndSetsReservedAt()
    {
        // Arrange
        await SeedDefaultTestData();
        
        var request = new UpdateTableStatusRequest(TableStatus.Reserved);
        var beforeUpdate = DateTime.UtcNow;

        // Act
        var response = await Client.PutAsync("/api/tables/1/status", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var tableResponse = await HttpHelpers.DeserializeResponse<UpdateTableStatusResponse>(response);
        tableResponse.Should().NotBeNull();
        tableResponse!.Id.Should().Be(1);
        tableResponse.TableNumber.Should().Be(1);
        tableResponse.Status.Should().Be("Reserved");
        
        // Verify table was updated in database and ReservedAt was set
        using var dbContext = GetDbContext();
        var updatedTable = await dbContext.Tables.FindAsync(1);
        updatedTable.Should().NotBeNull();
        updatedTable!.Status.Should().Be(TableStatus.Reserved);
        updatedTable.ReservedAt.Should().NotBeNull();
        updatedTable.ReservedAt.Should().BeCloseTo(beforeUpdate, TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task UpdateTableStatus_ToCleaning_UpdatesSuccessfully()
    {
        // Arrange
        await SeedDefaultTestData();
        
        var request = new UpdateTableStatusRequest(TableStatus.Cleaning);

        // Act
        var response = await Client.PutAsync("/api/tables/1/status", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var tableResponse = await HttpHelpers.DeserializeResponse<UpdateTableStatusResponse>(response);
        tableResponse.Should().NotBeNull();
        tableResponse!.Id.Should().Be(1);
        tableResponse.TableNumber.Should().Be(1);
        tableResponse.Status.Should().Be("Cleaning");
        
        // Verify table was updated in database
        using var dbContext = GetDbContext();
        var updatedTable = await dbContext.Tables.FindAsync(1);
        updatedTable.Should().NotBeNull();
        updatedTable!.Status.Should().Be(TableStatus.Cleaning);
    }

    [Test]
    public async Task UpdateTableStatus_WithNonExistentTable_ReturnsNotFound()
    {
        // Arrange
        await SeedDefaultTestData();
        
        var request = new UpdateTableStatusRequest(TableStatus.Available);

        // Act
        var response = await Client.PutAsync("/api/tables/999/status", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task UpdateTableStatus_WithInvalidTableId_ReturnsNotFound()
    {
        // Arrange
        await SeedDefaultTestData();
        
        var request = new UpdateTableStatusRequest(TableStatus.Available);

        // Act
        var response = await Client.PutAsync("/api/tables/999/status", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task UpdateTableStatus_FromReservedToAvailable_ClearsReservedAt()
    {
        // Arrange
        await SeedDefaultTestData();
        
        // First verify the table is reserved with ReservedAt set
        using (var initialDbContext = GetDbContext())
        {
            var table = await initialDbContext.Tables.FindAsync(3);
            table!.Status.Should().Be(TableStatus.Reserved);
            table.ReservedAt.Should().NotBeNull();
        }
        
        var request = new UpdateTableStatusRequest(TableStatus.Available);

        // Act
        var response = await Client.PutAsync("/api/tables/3/status", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify ReservedAt was cleared
        using var dbContext = GetDbContext();
        var updatedTable = await dbContext.Tables.FindAsync(3);
        updatedTable.Should().NotBeNull();
        updatedTable!.Status.Should().Be(TableStatus.Available);
        updatedTable.ReservedAt.Should().BeNull();
    }

    [Test]
    public async Task UpdateTableStatus_PreservesReservedAtWhenNotChangingToOrFromReserved()
    {
        // Arrange
        await SeedDefaultTestData();
        
        // Get the original ReservedAt value
        DateTime? originalReservedAt;
        using (var initialDbContext = GetDbContext())
        {
            var table = await initialDbContext.Tables.FindAsync(3);
            originalReservedAt = table!.ReservedAt;
        }
        
        var request = new UpdateTableStatusRequest(TableStatus.Occupied);

        // Act
        var response = await Client.PutAsync("/api/tables/3/status", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify ReservedAt was preserved
        using var dbContext = GetDbContext();
        var updatedTable = await dbContext.Tables.FindAsync(3);
        updatedTable.Should().NotBeNull();
        updatedTable!.Status.Should().Be(TableStatus.Occupied);
        updatedTable.ReservedAt.Should().Be(originalReservedAt);
    }

    [Test]
    public async Task UpdateTableStatus_MultipleUpdates_AllSucceed()
    {
        // Arrange
        await SeedDefaultTestData();

        // Act & Assert - Update table through different statuses
        var statuses = new[] { TableStatus.Occupied, TableStatus.Cleaning, TableStatus.Available, TableStatus.Reserved };
        
        foreach (var status in statuses)
        {
            var request = new UpdateTableStatusRequest(status);
            var response = await Client.PutAsync("/api/tables/1/status", HttpHelpers.ToJsonContent(request));
            
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var tableResponse = await HttpHelpers.DeserializeResponse<UpdateTableStatusResponse>(response);
            tableResponse!.Status.Should().Be(status.ToString());
        }

        // Verify final status in database
        using var dbContext = GetDbContext();
        var finalTable = await dbContext.Tables.FindAsync(1);
        finalTable!.Status.Should().Be(TableStatus.Reserved);
        finalTable.ReservedAt.Should().NotBeNull(); // Should be set for Reserved status
    }

    [Test]
    public async Task UpdateTableStatus_VerifiesResponseStructure()
    {
        // Arrange
        await SeedDefaultTestData();
        
        var request = new UpdateTableStatusRequest(TableStatus.Available);

        // Act
        var response = await Client.PutAsync("/api/tables/1/status", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        
        var tableResponse = await HttpHelpers.DeserializeResponse<UpdateTableStatusResponse>(response);
        tableResponse.Should().NotBeNull();
        tableResponse!.Id.Should().BeGreaterThan(0);
        tableResponse.TableNumber.Should().BeGreaterThan(0);
        tableResponse.Status.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task UpdateTableStatus_DoesNotAffectOtherTables()
    {
        // Arrange
        await SeedDefaultTestData();
        
        // Store original status of table 2
        using var originalDbContext = GetDbContext();
        var originalTable2 = await originalDbContext.Tables.FindAsync(2);
        var originalStatus = originalTable2!.Status;
        
        var request = new UpdateTableStatusRequest(TableStatus.Available);

        // Act - Update table 1
        var response = await Client.PutAsync("/api/tables/1/status", HttpHelpers.ToJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify table 2 wasn't affected
        using var dbContext = GetDbContext();
        var table2 = await dbContext.Tables.FindAsync(2);
        table2!.Status.Should().Be(originalStatus);
        
        // Verify table 1 was updated
        var table1 = await dbContext.Tables.FindAsync(1);
        table1!.Status.Should().Be(TableStatus.Available);
    }

    [Test]
    public async Task UpdateTableStatus_WithInvalidStatus_ReturnsBadRequest()
    {
        // Arrange
        await SeedDefaultTestData();
        
        // Create request with invalid JSON for enum
        var invalidJson = """{"status": "InvalidStatus"}""";
        var content = new StringContent(invalidJson, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PutAsync("/api/tables/1/status", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}