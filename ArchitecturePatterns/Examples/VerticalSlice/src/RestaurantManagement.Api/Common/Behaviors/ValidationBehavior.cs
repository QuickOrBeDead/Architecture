using FluentValidation;
using Mediator;
using RestaurantManagement.Api.Common;

namespace RestaurantManagement.Api.Common.Behaviors;

/// <summary>
/// Pipeline behavior for validating requests using FluentValidation.
/// This behavior runs before the request handler and validates the request object.
/// If validation fails, it throws a ValidationException with detailed error information.
/// </summary>
/// <typeparam name="TRequest">The type of the request to validate</typeparam>
/// <typeparam name="TResponse">The type of the response</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators,
    ILogger<ValidationBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        // If no validators are registered for this request type, skip validation
        if (!validators.Any())
        {
            return await next(message, cancellationToken);
        }

        var context = new ValidationContext<TRequest>(message);

        // Run all validators in parallel for better performance
        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Collect all validation failures
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
        {
            var requestName = typeof(TRequest).Name;
            var errorMessages = failures.Select(f => f.ErrorMessage).ToList();
            
            logger.LogDebug(
                "Validation failed for {RequestName}. Errors: {ValidationErrors}",
                requestName,
                string.Join("; ", errorMessages));

            // Create a dictionary of property errors for better error handling
            var propertyErrors = failures
                .GroupBy(f => f.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(f => f.ErrorMessage).ToArray());

            throw new ValidationException(failures);
        }

        // Log successful validation for debugging purposes
        logger.LogDebug("Validation passed for {RequestName}", typeof(TRequest).Name);

        return await next(message, cancellationToken);
    }
}