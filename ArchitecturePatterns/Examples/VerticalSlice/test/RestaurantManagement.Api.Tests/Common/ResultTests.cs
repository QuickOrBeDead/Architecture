using RestaurantManagement.Api.Common;

namespace RestaurantManagement.Api.Tests.Common;

[TestFixture]
public class ResultTests
{
    [Test]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Arrange
        const string testData = "test data";

        // Act
        var result = Result<string>.Success(testData);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(testData);
        result.ErrorMessage.Should().BeNull();
        result.ResultType.Should().Be(ResultType.Success);
        result.ErrorDetails.Should().BeEmpty();
    }

    [Test]
    public void Success_WithNullData_ShouldCreateSuccessfulResult()
    {
        // Act
        var result = Result<string?>.Success(null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().BeNull();
        result.ResultType.Should().Be(ResultType.Success);
        result.ErrorDetails.Should().BeEmpty();
    }

    [Test]
    public void Failure_WithDefaultResultType_ShouldCreateFailureResult()
    {
        // Arrange
        const string errorMessage = "Something went wrong";

        // Act
        var result = Result<string>.Failure(errorMessage);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Be(errorMessage);
        result.ResultType.Should().Be(ResultType.Failure);
        result.ErrorDetails.Should().BeEmpty();
    }

    [Test]
    public void Failure_WithSpecificResultType_ShouldCreateFailureResult()
    {
        // Arrange
        const string errorMessage = "Conflict occurred";
        const ResultType resultType = ResultType.Conflict;

        // Act
        var result = Result<string>.Failure(errorMessage, resultType);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Be(errorMessage);
        result.ResultType.Should().Be(resultType);
        result.ErrorDetails.Should().BeEmpty();
    }

    [Test]
    public void Failure_WithErrorDetails_ShouldCreateFailureResultWithDetails()
    {
        // Arrange
        const string errorMessage = "Validation failed";
        var errorDetails = new Dictionary<string, object>
        {
            { "Field1", "Error 1" },
            { "Field2", new[] { "Error 2", "Error 3" } }
        };

        // Act
        var result = Result<string>.Failure(errorMessage, ResultType.Failure, errorDetails);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Be(errorMessage);
        result.ResultType.Should().Be(ResultType.Failure);
        result.ErrorDetails.Should().BeEquivalentTo(errorDetails);
    }

    [Test]
    public void NotFound_ShouldCreateNotFoundResult()
    {
        // Arrange
        const string errorMessage = "Resource not found";

        // Act
        var result = Result<string>.NotFound(errorMessage);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Be(errorMessage);
        result.ResultType.Should().Be(ResultType.NotFound);
        result.ErrorDetails.Should().BeEmpty();
    }

    [Test]
    public void NotFound_WithErrorDetails_ShouldCreateNotFoundResultWithDetails()
    {
        // Arrange
        const string errorMessage = "User not found";
        var errorDetails = new Dictionary<string, object>
        {
            { "UserId", 123 },
            { "SearchCriteria", "email@example.com" }
        };

        // Act
        var result = Result<string>.NotFound(errorMessage, errorDetails);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Be(errorMessage);
        result.ResultType.Should().Be(ResultType.NotFound);
        result.ErrorDetails.Should().BeEquivalentTo(errorDetails);
    }

    [Test]
    public void Conflict_ShouldCreateConflictResult()
    {
        // Arrange
        const string errorMessage = "Resource already exists";

        // Act
        var result = Result<string>.Conflict(errorMessage);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Be(errorMessage);
        result.ResultType.Should().Be(ResultType.Conflict);
        result.ErrorDetails.Should().BeEmpty();
    }

    [Test]
    public void Conflict_WithErrorDetails_ShouldCreateConflictResultWithDetails()
    {
        // Arrange
        const string errorMessage = "Email already exists";
        var errorDetails = new Dictionary<string, object>
        {
            { "Email", "test@example.com" },
            { "ExistingUserId", 456 }
        };

        // Act
        var result = Result<string>.Conflict(errorMessage, errorDetails);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Be(errorMessage);
        result.ResultType.Should().Be(ResultType.Conflict);
        result.ErrorDetails.Should().BeEquivalentTo(errorDetails);
    }

    [Test]
    public void ValidationFailure_ShouldCreateValidationFailureResult()
    {
        // Arrange
        var errorMessages = new List<string> { "Field1 is required", "Field2 must be valid" };
        var propertyErrors = new Dictionary<string, string[]>
        {
            { "Field1", new[] { "Field1 is required" } },
            { "Field2", new[] { "Field2 must be valid", "Field2 length must be between 1 and 10" } }
        };

        // Act
        var result = Result<string>.ValidationFailure(errorMessages, propertyErrors);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Be("Validation failed: Field1 is required; Field2 must be valid");
        result.ResultType.Should().Be(ResultType.Failure);
        
        result.ErrorDetails.Should().HaveCount(2);
        result.ErrorDetails["Field1"].Should().BeEquivalentTo(new[] { "Field1 is required" });
        result.ErrorDetails["Field2"].Should().BeEquivalentTo(new[] { "Field2 must be valid", "Field2 length must be between 1 and 10" });
    }

    [Test]
    public void ValidationFailure_WithEmptyErrorMessages_ShouldCreateValidationFailureResult()
    {
        // Arrange
        var errorMessages = new List<string>();
        var propertyErrors = new Dictionary<string, string[]>
        {
            { "Field1", new[] { "Field1 is required" } }
        };

        // Act
        var result = Result<string>.ValidationFailure(errorMessages, propertyErrors);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Validation failed: ");
        result.ResultType.Should().Be(ResultType.Failure);
        result.ErrorDetails["Field1"].Should().BeEquivalentTo(new[] { "Field1 is required" });
    }

    [Test]
    public void ErrorDetails_WhenNotInitialized_ShouldReturnEmptyDictionary()
    {
        // Act
        var result = Result<string>.Success("test");

        // Assert
        result.ErrorDetails.Should().BeEmpty();
        result.ErrorDetails.Should().NotBeNull();
    }

    [Test]
    public void ErrorDetails_WhenAccessedMultipleTimes_ShouldReturnSameInstance()
    {
        // Act
        var result = Result<string>.Success("test");
        var errorDetails1 = result.ErrorDetails;
        var errorDetails2 = result.ErrorDetails;

        // Assert
        errorDetails1.Should().BeSameAs(errorDetails2);
    }

    [Test]
    public void Result_WithComplexDataType_ShouldWork()
    {
        // Arrange
        var testObject = new TestClass { Id = 1, Name = "Test" };

        // Act
        var result = Result<TestClass>.Success(testObject);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(testObject);
    }

    [Test]
    public void Result_ImplementsIResult_ShouldExposeCorrectProperties()
    {
        // Arrange
        const string errorMessage = "Test error";
        var result = Result<string>.Failure(errorMessage, ResultType.NotFound);

        // Act
        IResult iResult = result;

        // Assert
        iResult.IsSuccess.Should().BeFalse();
        iResult.ErrorMessage.Should().Be(errorMessage);
        iResult.ResultType.Should().Be(ResultType.NotFound);
        iResult.ErrorDetails.Should().BeEmpty();
    }

    private class TestClass
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}