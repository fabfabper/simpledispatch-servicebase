namespace SimpleDispatch.ServiceBase.Models;

/// <summary>
/// Standard API response model
/// </summary>
/// <typeparam name="T">Type of the response data</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Response message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Response data
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Error details if operation failed
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Timestamp of the response
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Create a successful response
    /// </summary>
    public static ApiResponse<T> CreateSuccess(T data, string message = "Operation successful")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// Create an error response
    /// </summary>
    public static ApiResponse<T> CreateError(string error, string message = "Operation failed")
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Error = error
        };
    }
}
