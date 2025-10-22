# RestaurantManagement.Api.Tests

This project contains unit tests for the RestaurantManagement API, specifically focusing on testing the classes in the `Common` folder.

## Test Coverage

### Common Folder Tests

#### Result Class Tests (`ResultTests.cs`)

- Tests all factory methods for creating success and failure results
- Tests all result types (Success, NotFound, Conflict, Failure)
- Tests validation failure result creation with structured error details
- Tests the IResult interface implementation
- Tests error details lazy initialization and behavior

#### ResultHelper Class Tests (`ResultHelperTests.cs`)

- Tests the ToApiResult extension method for converting Result objects to ASP.NET Core IResult responses
- Tests different status code scenarios (200 OK, 201 Created, 204 No Content)
- Tests custom success result handling
- Tests the CreateValidationFailure generic method for creating validation errors

#### ValidationBehavior Tests (`ValidationBehaviorTests.cs`)

- Tests the MediatR pipeline behavior for request validation
- Tests scenarios with no validators, valid requests, and validation errors
- Tests error aggregation from multiple validators
- Tests logging behavior for both success and failure scenarios
- Tests cancellation token propagation to validators
- Tests null validation failure filtering

## Test Framework

The tests use:

- **NUnit** - Testing framework
- **FluentAssertions** - Assertion library for more readable test assertions
- **Moq** - Mocking framework for creating test doubles
- **.NET 9.0** - Target framework

## Running Tests

To run all tests in this project:

```bash
dotnet test RestaurantManagement.Api.Tests.csproj
```

To run tests with detailed output:

```bash
dotnet test RestaurantManagement.Api.Tests.csproj --verbosity normal
```

To run a specific test class:

```bash
dotnet test RestaurantManagement.Api.Tests.csproj --filter "FullyQualifiedName~ResultTests"
```

## Dependencies

The test project references:

- The main `RestaurantManagement.Api` project for testing its classes
- FluentValidation for validation testing
- Mediator.Abstractions for pipeline behavior testing
- Microsoft.Extensions.Logging.Abstractions for logging testing
- Microsoft.AspNetCore.Http for HTTP result testing

## Test Structure

Tests follow the AAA (Arrange, Act, Assert) pattern and use descriptive naming that clearly indicates:

- The method or scenario being tested
- The expected behavior or outcome
- Any specific conditions or edge cases