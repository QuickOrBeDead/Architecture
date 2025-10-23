# RestaurantManagement.Api.FunctionalTests

This project contains comprehensive functional tests for all API endpoints in the Restaurant Management system using NUnit framework.

## Overview

The functional tests verify the complete behavior of API endpoints by testing them end-to-end through HTTP requests. These tests use an in-memory database for isolation and run against the actual ASP.NET Core application.

## Test Structure

### Infrastructure
- **RestaurantTestWebApplicationFactory**: Custom WebApplicationFactory that configures the test server with an isolated in-memory database
- **FunctionalTestBase**: Base class for all test classes providing common setup, teardown, and utility methods
- **HttpHelpers**: Helper methods for HTTP operations and JSON serialization/deserialization

### Test Coverage

#### MenuItems Endpoints
- **GetMenuItemsEndpointTests**: Tests for `GET /api/menuitems`
  - Returns all available menu items
  - Filters by category
  - Handles empty database
  - Validates response structure
  - Case sensitivity tests

#### Orders Endpoints
- **CreateOrderEndpointTests**: Tests for `POST /api/orders`
  - Creates orders with valid data
  - Validates table availability
  - Handles invalid menu items
  - Tests validation rules (empty items, invalid quantities, etc.)
  - Verifies total amount calculations

- **GetKitchenOrdersEndpointTests**: Tests for `GET /api/orders/kitchen`
  - Returns active orders for kitchen display
  - Verifies order and item details
  - Handles empty database
  - Tests special instructions handling

- **UpdateOrderStatusEndpointTests**: Tests for `PUT /api/orders/{id}/status`
  - Updates order status through all states
  - Handles non-existent orders
  - Verifies database persistence
  - Tests multiple status transitions

#### Tables Endpoints
- **GetAllTablesEndpointTests**: Tests for `GET /api/tables`
  - Returns all tables ordered by table number
  - Verifies table details and statuses
  - Handles ReservedAt field correctly
  - Tests empty database scenarios

- **UpdateTableStatusEndpointTests**: Tests for `PUT /api/tables/{id}/status`
  - Updates table status through all states
  - Manages ReservedAt field correctly
  - Handles non-existent tables
  - Tests status transition logic

## Key Features

### Database Isolation
Each test uses a unique in-memory database to ensure complete isolation between tests.

### Comprehensive Validation Testing
Tests cover all validation scenarios including:
- Required fields
- Field length limits
- Business rule validations
- Invalid enum values
- Non-existent entity references

### Real HTTP Testing
Tests make actual HTTP requests to verify:
- Correct HTTP status codes
- Proper response content types
- Response structure validation
- Error handling

### Test Data Management
- **SeedDefaultTestData()**: Provides consistent test data across test classes
- **SeedDatabase()**: Custom seeding for specific test scenarios
- **GetDbContext()**: Access to database context for assertions

## Running the Tests

```bash
# Run all functional tests
dotnet test RestaurantManagement.Api.FunctionalTests.csproj

# Run tests for a specific endpoint
dotnet test --filter "FullyQualifiedName~GetMenuItemsEndpointTests"

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"
```

## Test Data

### Default Test Data Includes:
- **Tables**: 3 tables with different statuses (Available, Occupied, Reserved)
- **Menu Items**: 5 menu items across different categories (Pizza, Salad, Main Course, Dessert)
- **Orders**: 2 sample orders with different statuses and items
- **Order Items**: Related order items with various menu items and special instructions

## Dependencies

- **NUnit**: Test framework
- **FluentAssertions**: Assertion library for readable tests
- **Microsoft.AspNetCore.Mvc.Testing**: ASP.NET Core testing utilities
- **Microsoft.EntityFrameworkCore.InMemory**: In-memory database provider

## Best Practices Implemented

1. **Test Isolation**: Each test runs with a fresh database
2. **Descriptive Test Names**: Clear test method names describing the scenario
3. **AAA Pattern**: Arrange, Act, Assert structure in all tests
4. **Comprehensive Coverage**: Tests cover both happy path and error scenarios
5. **Real HTTP Requests**: Tests use actual HTTP clients for realistic testing
6. **Database Verification**: Tests verify both HTTP responses and database state
7. **Edge Case Testing**: Tests handle boundary conditions and invalid inputs