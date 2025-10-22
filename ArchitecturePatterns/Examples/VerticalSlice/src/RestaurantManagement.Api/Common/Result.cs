namespace RestaurantManagement.Api.Common;

public enum ResultType
{
    Success,
    NotFound,
    Conflict,
    Failure
}

public sealed class Result<T>
{
    private Dictionary<string, object>? _errorDetails;
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string? ErrorMessage { get; private set; }
    public ResultType ResultType { get; private set; }

    public Dictionary<string, object> ErrorDetails
    {
        get
        {
            return _errorDetails ??= [];
        }
        private set
        {
            _errorDetails = value;
        }
    }

    private Result() { }

    public static Result<T> Success(T data)
    {
        return new Result<T>
        {
            IsSuccess = true,
            Data = data,
            ResultType = ResultType.Success
        };
    }

    public static Result<T> Failure(string errorMessage, ResultType resultType = ResultType.Failure, Dictionary<string, object>? errorDetails = null)
    {
        return new Result<T>
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ResultType = resultType,
            _errorDetails = errorDetails
        };
    }

    public static Result<T> NotFound(string errorMessage, Dictionary<string, object>? errorDetails = null)
    {
        return Failure(errorMessage, ResultType.NotFound, errorDetails);
    }

    public static Result<T> Conflict(string errorMessage, Dictionary<string, object>? errorDetails = null)
    {
        return Failure(errorMessage, ResultType.Conflict, errorDetails);
    }
}