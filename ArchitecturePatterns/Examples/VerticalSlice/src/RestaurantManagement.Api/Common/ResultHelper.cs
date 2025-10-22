namespace RestaurantManagement.Api.Common;

public static class ResultHelper
{
    /// <summary>
    /// Converts a Result to an appropriate IResult for API responses
    /// </summary>
    /// <typeparam name="T">The type of data in the result</typeparam>
    /// <param name="result">The result to convert</param>
    /// <param name="successStatusCode">The status code to return on success (default: 200 OK)</param>
    /// <returns>An IResult with appropriate status code and error details</returns>
    public static IResult ToApiResult<T>(this Result<T> result, int successStatusCode = StatusCodes.Status200OK)
    {
        if (result.IsSuccess)
        {
            return successStatusCode switch
            {
                StatusCodes.Status200OK => Results.Ok(result.Data),
                StatusCodes.Status201Created => Results.Created(string.Empty, result.Data),
                StatusCodes.Status204NoContent => Results.NoContent(),
                _ => Results.Ok(result.Data)
            };
        }

        var errorDetails = new
        {
            error = result.ErrorMessage,
            errorDetails = result.ErrorDetails
        };

        return result.ResultType switch
        {
            ResultType.NotFound => Results.NotFound(errorDetails),
            ResultType.Conflict => Results.Conflict(errorDetails),
            ResultType.Failure => Results.BadRequest(errorDetails),
            _ => Results.BadRequest(errorDetails)
        };
    }

    /// <summary>
    /// Converts a Result to an appropriate IResult for API responses with a custom success result
    /// </summary>
    /// <typeparam name="T">The type of data in the result</typeparam>
    /// <param name="result">The result to convert</param>
    /// <param name="successResult">The IResult to return on success</param>
    /// <returns>An IResult with appropriate status code and error details</returns>
    public static IResult ToApiResult<T>(this Result<T> result, Func<T?, IResult> successResult)
    {
        if (result.IsSuccess)
        {
            return successResult(result.Data);
        }

        var errorDetails = new
        {
            error = result.ErrorMessage,
            errorDetails = result.ErrorDetails
        };

        return result.ResultType switch
        {
            ResultType.NotFound => Results.NotFound(errorDetails),
            ResultType.Conflict => Results.Conflict(errorDetails),
            ResultType.Failure => Results.BadRequest(errorDetails),
            _ => Results.BadRequest(errorDetails)
        };
    }
}