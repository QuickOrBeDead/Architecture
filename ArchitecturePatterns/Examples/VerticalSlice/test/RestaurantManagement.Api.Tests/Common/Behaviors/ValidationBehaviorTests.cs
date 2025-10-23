using FluentValidation;
using FluentValidation.Results;
using Mediator;
using Microsoft.Extensions.Logging;
using RestaurantManagement.Api.Common;
using RestaurantManagement.Api.Common.Behaviors;

namespace RestaurantManagement.Api.Tests.Common.Behaviors;

[TestFixture]
public class ValidationBehaviorTests
{
    private Mock<ILogger<ValidationBehavior<TestRequest, TestResponse>>> _loggerMock;
    private Mock<IValidator<TestRequest>> _validator1Mock;
    private Mock<IValidator<TestRequest>> _validator2Mock;
    private Mock<MessageHandlerDelegate<TestRequest, Result<TestResponse>>> _nextMock;
    private TestRequest _request;
    private CancellationToken _cancellationToken;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<ValidationBehavior<TestRequest, TestResponse>>>();
        _validator1Mock = new Mock<IValidator<TestRequest>>();
        _validator2Mock = new Mock<IValidator<TestRequest>>();
        _nextMock = new Mock<MessageHandlerDelegate<TestRequest, Result<TestResponse>>>();
        _request = new TestRequest { Name = "Test", Age = 25 };
        _cancellationToken = CancellationToken.None;
    }

    [Test]
    public async Task Handle_WithNoValidators_ShouldCallNext()
    {
        // Arrange
        var validators = Array.Empty<IValidator<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators, _loggerMock.Object);
        var expectedResult = Result<TestResponse>.Success(new TestResponse());
        
        _nextMock.Setup(x => x(_request, _cancellationToken))
               .ReturnsAsync(expectedResult);

        // Act
        var result = await behavior.Handle(_request, _nextMock.Object, _cancellationToken);

        // Assert
        result.Should().BeSameAs(expectedResult);
        _nextMock.Verify(x => x(_request, _cancellationToken), Times.Once);
    }

    [Test]
    public async Task Handle_WithValidRequest_ShouldCallNext()
    {
        // Arrange
        var validators = new[] { _validator1Mock.Object, _validator2Mock.Object };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators, _loggerMock.Object);
        var expectedResult = Result<TestResponse>.Success(new TestResponse());

        _validator1Mock.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), _cancellationToken))
                      .ReturnsAsync(new ValidationResult());
        _validator2Mock.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), _cancellationToken))
                      .ReturnsAsync(new ValidationResult());
        _nextMock.Setup(x => x(_request, _cancellationToken))
               .ReturnsAsync(expectedResult);

        // Act
        var result = await behavior.Handle(_request, _nextMock.Object, _cancellationToken);

        // Assert
        result.Should().BeSameAs(expectedResult);
        _nextMock.Verify(x => x(_request, _cancellationToken), Times.Once);
    }

    [Test]
    public async Task Handle_WithValidationErrors_ShouldReturnValidationFailureResult()
    {
        // Arrange
        var validators = new[] { _validator1Mock.Object };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators, _loggerMock.Object);
        
        var validationFailures = new[]
        {
            new ValidationFailure("Name", "Name is required"),
            new ValidationFailure("Age", "Age must be positive"),
            new ValidationFailure("Age", "Age must be less than 120")
        };
        var validationResult = new ValidationResult(validationFailures);

        _validator1Mock.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), _cancellationToken))
                      .ReturnsAsync(validationResult);

        // Act
        var result = await behavior.Handle(_request, _nextMock.Object, _cancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ResultType.Should().Be(ResultType.Failure);
        result.ErrorMessage.Should().Be("Validation failed for TestRequest");
        
        result.ErrorDetails.Should().HaveCount(2);
        result.ErrorDetails["Name"].Should().BeEquivalentTo(new[] { "Name is required" });
        result.ErrorDetails["Age"].Should().BeEquivalentTo(new[] { "Age must be positive", "Age must be less than 120" });
        
        _nextMock.Verify(x => x(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_WithMultipleValidators_ShouldAggregateValidationErrors()
    {
        // Arrange
        var validators = new[] { _validator1Mock.Object, _validator2Mock.Object };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators, _loggerMock.Object);
        
        var validationFailures1 = new[]
        {
            new ValidationFailure("Name", "Name is required")
        };
        var validationFailures2 = new[]
        {
            new ValidationFailure("Age", "Age must be positive"),
            new ValidationFailure("Email", "Email is invalid")
        };

        _validator1Mock.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), _cancellationToken))
                      .ReturnsAsync(new ValidationResult(validationFailures1));
        _validator2Mock.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), _cancellationToken))
                      .ReturnsAsync(new ValidationResult(validationFailures2));

        // Act
        var result = await behavior.Handle(_request, _nextMock.Object, _cancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ResultType.Should().Be(ResultType.Failure);
        result.ErrorMessage.Should().Be("Validation failed for TestRequest");
        
        result.ErrorDetails.Should().HaveCount(3);
        result.ErrorDetails["Name"].Should().BeEquivalentTo(new[] { "Name is required" });
        result.ErrorDetails["Age"].Should().BeEquivalentTo(new[] { "Age must be positive" });
        result.ErrorDetails["Email"].Should().BeEquivalentTo(new[] { "Email is invalid" });
        
        _nextMock.Verify(x => x(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_WithMixedValidationResults_ShouldOnlyIncludeFailures()
    {
        // Arrange
        var validators = new[] { _validator1Mock.Object, _validator2Mock.Object };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators, _loggerMock.Object);
        
        var validationFailures = new[]
        {
            new ValidationFailure("Name", "Name is required")
        };

        _validator1Mock.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), _cancellationToken))
                      .ReturnsAsync(new ValidationResult(validationFailures));
        _validator2Mock.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), _cancellationToken))
                      .ReturnsAsync(new ValidationResult()); // Valid result

        // Act
        var result = await behavior.Handle(_request, _nextMock.Object, _cancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ResultType.Should().Be(ResultType.Failure);
        
        result.ErrorDetails.Should().HaveCount(1);
        result.ErrorDetails["Name"].Should().BeEquivalentTo(new[] { "Name is required" });
        
        _nextMock.Verify(x => x(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_WithValidationErrors_ShouldLogDebugMessage()
    {
        // Arrange
        var validators = new[] { _validator1Mock.Object };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators, _loggerMock.Object);
        
        var validationFailures = new[]
        {
            new ValidationFailure("Name", "Name is required")
        };
        var validationResult = new ValidationResult(validationFailures);

        _validator1Mock.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), _cancellationToken))
                      .ReturnsAsync(validationResult);

        // Act
        await behavior.Handle(_request, _nextMock.Object, _cancellationToken);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Validation failed for TestRequest")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_WithSuccessfulValidation_ShouldLogDebugMessage()
    {
        // Arrange
        var validators = new[] { _validator1Mock.Object };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators, _loggerMock.Object);
        var expectedResult = Result<TestResponse>.Success(new TestResponse());

        _validator1Mock.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), _cancellationToken))
                      .ReturnsAsync(new ValidationResult());
        _nextMock.Setup(x => x(_request, _cancellationToken))
               .ReturnsAsync(expectedResult);

        // Act
        await behavior.Handle(_request, _nextMock.Object, _cancellationToken);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Validation passed for TestRequest")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToValidators()
    {
        // Arrange
        var validators = new[] { _validator1Mock.Object };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators, _loggerMock.Object);
        var customCancellationToken = new CancellationTokenSource().Token;

        _validator1Mock.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), customCancellationToken))
                      .ReturnsAsync(new ValidationResult())
                      .Verifiable();

        var expectedResult = Result<TestResponse>.Success(new TestResponse());
        _nextMock.Setup(x => x(_request, customCancellationToken))
               .ReturnsAsync(expectedResult);

        // Act
        await behavior.Handle(_request, _nextMock.Object, customCancellationToken);

        // Assert
        _validator1Mock.Verify();
    }

    [Test]
    public async Task Handle_WithNullValidationFailures_ShouldFilterOutNulls()
    {
        // Arrange
        var validators = new[] { _validator1Mock.Object };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators, _loggerMock.Object);
        
        var validationFailures = new ValidationFailure[]
        {
            new("Name", "Name is required"),
            null!, // This should be filtered out
            new("Age", "Age must be positive")
        };
        var validationResult = new ValidationResult(validationFailures.Where(f => f != null));

        _validator1Mock.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), _cancellationToken))
                      .ReturnsAsync(validationResult);

        // Act
        var result = await behavior.Handle(_request, _nextMock.Object, _cancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorDetails.Should().HaveCount(2);
        result.ErrorDetails["Name"].Should().BeEquivalentTo(new[] { "Name is required" });
        result.ErrorDetails["Age"].Should().BeEquivalentTo(new[] { "Age must be positive" });
    }

    // Test helper classes
    public class TestRequest : IRequest<Result<TestResponse>>
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    public class TestResponse
    {
        public string Message { get; set; } = "Success";
    }
}