# RestaurantManagement.Api.IntegrationTests

This project contains comprehensive integration tests for the RestaurantManagement.Api application, covering all layers of the vertical slice architecture.

## Integration Test Purpose

**Integration tests focus on testing internal component interactions without the HTTP layer.** 
They verify that different layers of the application work correctly together at the **code level**, 
not through the public API (which is covered by functional tests).

## What Integration Tests Should Cover (vs Functional Tests)

| **Integration Tests** | **Functional Tests** |
|----------------------|---------------------|
| 🔧 **Internal component wiring** | 🌐 **HTTP API contracts** |
| 🏗️ **Database + Domain logic** | 📡 **Request/Response flows** |
| ⚙️ **MediatR + EF Core pipeline** | 🔍 **User workflow scenarios** |
| 🔗 **Service dependencies** | 🎯 **Business acceptance criteria** |
| 🧪 **Infrastructure components** | 📋 **End-to-end feature validation** |

## Integration Test Categories

### 🏗️ **Data Layer Integration** (`Data/`) ✅ **UNIQUE VALUE**
- **Database schema and model validation** (not HTTP-accessible)
- **Entity relationship integrity** (database-level constraints)
- **EF Core configuration verification** (mapping, indexes, constraints)
- **Database performance and connection handling**
- **Concurrent access patterns**
- **Transaction behavior and rollback scenarios**

### 🔄 **Internal Pipeline Integration** (`Features/`) ✅ **UNIQUE VALUE**  
- **MediatR pipeline without HTTP** (Request → Handler → Database)
- **Business logic + Data access integration** (bypassing controllers)
- **Domain service + Repository interactions** 
- **Cross-cutting concerns** (logging, caching, validation behaviors)
- **Service layer component wiring**

### ✅ **Validation Pipeline Integration** (`Validation/`) ✅ **UNIQUE VALUE**
- **FluentValidation + MediatR behavior pipeline** (internal flow)
- **Validation rules + Database constraints** interaction
- **Business rule validation integration** (domain + persistence)
- **Error propagation through internal layers** (not HTTP errors)

### ⚡ **Infrastructure Integration** (`Performance/`) ✅ **UNIQUE VALUE**
- **Database connection pooling and performance**
- **Memory usage patterns of internal components**
- **Service dependency performance** (without network overhead)
- **Component interaction under load** (non-HTTP bottlenecks)

## Test Infrastructure

### 📋 **IntegrationTestBase**
- Provides SQLite database setup with temporary files
- Automatic database creation/cleanup per test
- Service provider configuration
- Helper methods for common operations

### 🎯 **Test Isolation**
- Each test gets fresh database instance
- No test interdependencies
- Parallel test execution support
- Clean setup/teardown lifecycle

## Running Tests

```bash
# Run all integration tests
dotnet test

# Run specific test category
dotnet test --filter "Category=Data"
dotnet test --filter "Category=Features"
dotnet test --filter "Category=Performance"

# Run with detailed output
dotnet test --verbosity detailed
```

## Integration Testing Philosophy

These integration tests follow the **Vertical Slice Architecture** principles:

### ✅ **Complete Feature Testing**
Each feature is tested as a complete vertical slice from API endpoint to database, ensuring:
- Request processing works end-to-end
- Business logic is correctly implemented
- Data persistence operates correctly
- Validation rules are enforced

### ✅ **Real Infrastructure**
Tests use real infrastructure components:
- Real SQLite database (not mocks)
- Real HTTP server for API tests
- Real MediatR pipeline
- Real validation pipeline

### ✅ **Business Scenario Coverage**
Tests cover realistic business scenarios:
- Complete order workflows (reserve table → create order → update status → serve)
- Error conditions and edge cases
- Concurrent operations
- Data consistency validation

## Key Integration Test Scenarios

1. **Table Management Workflow**
   - Get all tables → Reserve table → Update status → Release table

2. **Order Processing Workflow**
   - Create order → Validate items → Update status → Complete order

3. **Cross-Feature Scenarios**
   - Table reservation with order creation
   - Menu item availability checking
   - Order status transitions

4. **Error Handling**
   - Invalid data validation
   - Business rule violations
   - Concurrency conflicts
   - Resource not found scenarios

5. **Performance Scenarios**
   - Multiple concurrent orders
   - Large order processing
   - Query performance under load
   - Memory usage monitoring

The tests use SQLite with temporary files for fast, isolated, and reliable testing while maintaining realistic database behavior.
