namespace ExaminationSystem.API.Models.Responses;

/// <summary>
/// Base class for all API responses.
/// </summary>
/// <typeparam name="T">The type of the data returned.</typeparam>
public abstract class ApiResponse<T>
{
    /// <summary>
    /// Indicates whether the request was successful.
    /// </summary>
    public bool Success { get; set; } = false;

    /// <summary>
    /// The response payload.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// A human-readable message describing the result.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The application-specific error code, if any.
    /// </summary>
    public ApiErrorCode ErrorCode { get; set; } = ApiErrorCode.None;

    /// <summary>
    /// The UTC timestamp when the response was generated.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}