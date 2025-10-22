# Architecture Tests Documentation

## Overview

This project includes comprehensive architecture tests using **NetArchTest.Rules** and **NUnit** to enforce the vertical slice architecture pattern. These tests help maintain code quality, consistency, and architectural integrity.

## Test Categories

### 1. Dependency Architecture Tests (`DependencyArchitectureTests.cs`)

These tests enforce proper dependency management and isolation between components:

- ✅ **Features should not depend on each other**: Ensures each vertical slice is independent
- ✅ **Common should not depend on Features**: Maintains dependency direction (Common ← Features)
- ✅ **Data should not depend on Features**: Prevents circular dependencies
- ✅ **Entities should not depend on Features**: Keeps domain models pure
- ✅ **Entities should be isolated**: Domain entities don't depend on infrastructure
- ✅ **Features avoid forbidden namespaces**: Prevents use of outdated technologies

### 2. Design Pattern Tests (`DesignPatternTests.cs`)

These tests ensure proper implementation of CQRS and MediatR patterns:

- ✅ **Handlers implement IRequestHandler**: All handlers follow MediatR pattern
- ✅ **Queries implement IRequest**: Query objects are properly structured
- ✅ **Commands implement IRequest**: Command objects follow CQRS pattern
- ✅ **Queries/Commands are records**: Ensures immutability
- ✅ **DTOs/Responses are records**: Data transfer objects are immutable
- ✅ **Validators inherit AbstractValidator**: FluentValidation pattern compliance
- ✅ **Endpoints are static classes**: Minimal API endpoint pattern

### 3. Naming Convention Tests (`NamingConventionTests.cs`)

These tests enforce consistent naming patterns across features:

- ✅ **Handlers end with "Handler"**: Clear identification of handler classes
- ✅ **Endpoints end with "Endpoint"**: Consistent endpoint class naming
- ✅ **Queries end with "Query"**: CQRS query naming convention
- ✅ **Commands end with "Command"**: CQRS command naming convention
- ✅ **Validators end with "Validator"**: FluentValidation naming pattern
- ✅ **Responses end with "Response"**: Consistent response object naming
- ✅ **DTOs end with "Dto"**: Data transfer object naming

### 4. Structural Tests (`StructuralTests.cs`)

These tests verify the vertical slice structure integrity:

- ✅ **Each feature has at least one endpoint**: Complete feature implementation
- ✅ **Each feature has at least one handler**: Business logic implementation
- ✅ **Each feature has requests**: CQRS query/command implementation
- ✅ **Features are not empty**: No incomplete implementations
- ✅ **Handlers match request types**: Proper handler-request relationships
- ✅ **Features don't share types**: Maintains slice independence
- ✅ **Core types follow naming**: Handler/Endpoint/Query/Command naming

### 5. Architecture Test Suite (`ArchitectureTestSuite.cs`)

A comprehensive meta-test that runs all architecture rules and provides a summary report with:

- Pass/fail statistics
- Detailed violation reporting
- Compliance rate calculation (requires ≥80% compliance)

## Running the Tests

### Command Line

```bash
dotnet test src\RestaurantManagement.Api.ArchTests\RestaurantManagement.Api.ArchTests.csproj
```

### Visual Studio

1. Open Test Explorer
2. Run all tests in `RestaurantManagement.Api.ArchTests`

### Continuous Integration

Add to your CI/CD pipeline to enforce architecture rules automatically:

```yaml
- name: Run Architecture Tests
  run: dotnet test src/RestaurantManagement.Api.ArchTests/ --logger "console;verbosity=normal"
```

## Current Compliance

**Overall Compliance: 97% (32/33 tests passing)**
The architecture is well-structured and follows vertical slice principles with only minor naming convention adjustments needed.

## Benefits

### Code Quality

- **Consistency**: Enforces uniform patterns across all features
- **Maintainability**: Clear architectural boundaries prevent technical debt
- **Testability**: Isolated slices are easier to test and mock

### Team Productivity  

- **Clear Guidelines**: Developers know exactly how to structure new features
- **Fast Feedback**: CI pipeline catches violations immediately
- **Documentation**: Tests serve as executable architecture documentation

### Architecture Integrity

- **Dependency Management**: Prevents circular dependencies and coupling
- **Pattern Compliance**: Ensures CQRS, MediatR, and Clean Architecture patterns
- **Scalability**: Vertical slices can be extracted to microservices easily

## Adding New Features

When adding new features, follow this structure:

```
Features/
  FeatureName/
    OperationName/
      OperationNameEndpoint.cs      # Static class, Map{OperationName} method
      OperationNameHandler.cs       # Implements IRequestHandler<,>
      OperationNameQuery.cs         # Record implementing IRequest<>
      OperationNameResponse.cs      # Record with response data
      OperationNameValidator.cs     # Inherits AbstractValidator<>
      [SupportingDtos.cs]          # Additional DTOs as needed
```

The architecture tests will automatically validate the new feature follows all established patterns.

## Customization

To modify the rules:

1. Update the test methods in the respective test classes
2. Adjust allowed/forbidden dependencies as needed
3. Modify naming patterns to match your team's preferences
4. Update compliance thresholds in `ArchitectureTestSuite`

## Troubleshooting

### Common Violations

1. **Missing Handler**: Add a class ending with "Handler" implementing `IRequestHandler<,>`
2. **Wrong Dependencies**: Check that features only depend on Common, Data, Entities
3. **Naming Mismatch**: Ensure Handler/Query/Command names match their operation
4. **Missing Endpoint**: Add a static class with Map method for the feature

### Test Failures

Run individual test classes to isolate issues:

```bash
dotnet test --filter "ClassName=DependencyArchitectureTests"
```

The architecture tests provide detailed failure messages with specific type names and violations to help quickly identify and fix issues.
