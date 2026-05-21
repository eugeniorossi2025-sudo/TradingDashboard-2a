namespace WebApi.Models;

/// <summary>
/// Represents a standardized API response.
/// </summary>
/// <typeparam name="T">The type of data being returned.</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the message describing the result.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the data payload.
    /// </summary>
    public T Data { get; set; }

    /// <summary>
    /// Gets or sets the list of errors, if any.
    /// </summary>
    public List<string> Errors { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the response.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Creates a successful response.
    /// </summary>
    /// <param name="data">The data to return.</param>
    /// <param name="message">The success message.</param>
    /// <returns>A successful API response.</returns>
    public static ApiResponse<T> SuccessResponse(T data, string message = "Operation successful")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Errors = new List<string>(),
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates an error response.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errors">The list of errors.</param>
    /// <returns>An error API response.</returns>
    public static ApiResponse<T> ErrorResponse(string message, List<string> errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default,
            Errors = errors ?? new List<string>(),
            Timestamp = DateTime.UtcNow
        };
    }
}