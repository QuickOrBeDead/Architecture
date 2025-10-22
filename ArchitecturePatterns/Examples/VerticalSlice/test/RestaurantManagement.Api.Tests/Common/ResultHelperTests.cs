using Microsoft.AspNetCore.Http;
using RestaurantManagement.Api.Common;

namespace RestaurantManagement.Api.Tests.Common;

[TestFixture]
public class ResultHelperTests
{
    [Test]
    public void ToApiResult_WithSuccessfulResult_ShouldReturnValidResult()
    {
        // Arrange
        const string data = "test data";
        var result = Result<string>.Success(data);

        // Act
        var apiResult = result.ToApiResult();

        // Assert
        apiResult.Should().NotBeNull();
    }

    [Test]
    public void ToApiResult_WithSuccessfulResultAndCreatedStatusCode_ShouldReturnValidResult()
    {
        // Arrange
        const string data = "created data";
        var result = Result<string>.Success(data);

        // Act
        var apiResult = result.ToApiResult(StatusCodes.Status201Created);

        // Assert
        apiResult.Should().NotBeNull();
    }

    [Test]
    public void ToApiResult_WithSuccessfulResultAndNoContentStatusCode_ShouldReturnValidResult()
    {
        // Arrange
        var result = Result<string>.Success("data");

        // Act
        var apiResult = result.ToApiResult(StatusCodes.Status204NoContent);

        // Assert
        apiResult.Should().NotBeNull();
    }

    [Test]
    public void ToApiResult_WithSuccessfulResultAndCustomStatusCode_ShouldReturnValidResult()
    {
        // Arrange
        const string data = "test data";
        var result = Result<string>.Success(data);

        // Act
        var apiResult = result.ToApiResult(StatusCodes.Status202Accepted);

        // Assert
        apiResult.Should().NotBeNull();
    }

    [Test]
    public void ToApiResult_WithNotFoundResult_ShouldReturnValidResult()
    {
        // Arrange
        const string errorMessage = "Resource not found";
        var errorDetails = new Dictionary<string, object> { { "Id", 123 } };
        var result = Result<string>.NotFound(errorMessage, errorDetails);

        // Act
        var apiResult = result.ToApiResult();

        // Assert
        apiResult.Should().NotBeNull();
    }

    [Test]
    public void ToApiResult_WithConflictResult_ShouldReturnValidResult()
    {
        // Arrange
        const string errorMessage = "Resource conflict";
        var result = Result<string>.Conflict(errorMessage);

        // Act
        var apiResult = result.ToApiResult();

        // Assert
        apiResult.Should().NotBeNull();
    }

    [Test]
    public void ToApiResult_WithFailureResult_ShouldReturnValidResult()
    {
        // Arrange
        const string errorMessage = "Validation failed";
        var result = Result<string>.Failure(errorMessage);

        // Act
        var apiResult = result.ToApiResult();

        // Assert
        apiResult.Should().NotBeNull();
    }

    [Test]
    public void ToApiResult_WithCustomSuccessResult_WhenSuccessful_ShouldReturnCustomResult()
    {
        // Arrange
        const string data = "test data";
        var result = Result<string>.Success(data);
        Microsoft.AspNetCore.Http.IResult customSuccessResult = Results.Accepted("location", data);

        // Act
        var apiResult = result.ToApiResult(_ => customSuccessResult);

        // Assert
        apiResult.Should().BeSameAs(customSuccessResult);
    }

    [Test]
    public void ToApiResult_WithCustomSuccessResult_WhenFailed_ShouldReturnValidErrorResult()
    {
        // Arrange
        const string errorMessage = "Test error";
        var result = Result<string>.Failure(errorMessage, ResultType.NotFound);
        Microsoft.AspNetCore.Http.IResult customSuccessResult = Results.Accepted("location", "data");

        // Act
        var apiResult = result.ToApiResult(_ => customSuccessResult);

        // Assert
        apiResult.Should().NotBeNull();
        apiResult.Should().NotBeSameAs(customSuccessResult);
    }

    [Test]
    public void ToApiResult_WithCustomSuccessResult_ShouldPassDataToSuccessFunction()
    {
        // Arrange
        const string expectedData = "expected data";
        var result = Result<string>.Success(expectedData);
        string? capturedData = null;

        // Act
        var apiResult = result.ToApiResult(data =>
        {
            capturedData = data;
            return Results.Ok(data);
        });

        // Assert
        capturedData.Should().Be(expectedData);
    }

    [Test]
    public void CreateValidationFailure_ShouldCreateValidationFailureResult()
    {
        // Arrange
        var errorMessages = new List<string> { "Error 1", "Error 2" };
        var propertyErrors = new Dictionary<string, string[]>
        {
            { "Property1", new[] { "Property1 error" } },
            { "Property2", new[] { "Property2 error1", "Property2 error2" } }
        };

        // Act
        var result = ResultHelper.CreateValidationFailure<string>(errorMessages, propertyErrors);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Validation failed: Error 1; Error 2");
        result.ResultType.Should().Be(ResultType.Failure);
        result.ErrorDetails.Should().HaveCount(2);
        result.ErrorDetails["Property1"].Should().BeEquivalentTo(new[] { "Property1 error" });
        result.ErrorDetails["Property2"].Should().BeEquivalentTo(new[] { "Property2 error1", "Property2 error2" });
    }

    [Test]
    public void CreateValidationFailure_WithEmptyErrorMessages_ShouldCreateValidationFailureResult()
    {
        // Arrange
        var errorMessages = new List<string>();
        var propertyErrors = new Dictionary<string, string[]>
        {
            { "Property1", new[] { "Property1 error" } }
        };

        // Act
        var result = ResultHelper.CreateValidationFailure<int>(errorMessages, propertyErrors);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Validation failed: ");
        result.ResultType.Should().Be(ResultType.Failure);
        result.ErrorDetails.Should().HaveCount(1);
        result.ErrorDetails["Property1"].Should().BeEquivalentTo(new[] { "Property1 error" });
    }

    [Test]
    public void CreateValidationFailure_WithEmptyPropertyErrors_ShouldCreateValidationFailureResult()
    {
        // Arrange
        var errorMessages = new List<string> { "General error" };
        var propertyErrors = new Dictionary<string, string[]>();

        // Act
        var result = ResultHelper.CreateValidationFailure<bool>(errorMessages, propertyErrors);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Validation failed: General error");
        result.ResultType.Should().Be(ResultType.Failure);
        result.ErrorDetails.Should().BeEmpty();
    }

    [Test]
    public void CreateValidationFailure_WithComplexType_ShouldWork()
    {
        // Arrange
        var errorMessages = new List<string> { "Complex type validation error" };
        var propertyErrors = new Dictionary<string, string[]>
        {
            { "ComplexProperty", new[] { "Complex property error" } }
        };

        // Act
        var result = ResultHelper.CreateValidationFailure<TestComplexType>(errorMessages, propertyErrors);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Be("Validation failed: Complex type validation error");
        result.ResultType.Should().Be(ResultType.Failure);
        result.ErrorDetails["ComplexProperty"].Should().BeEquivalentTo(new[] { "Complex property error" });
    }

    private class TestComplexType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> Items { get; set; } = new();
    }
}