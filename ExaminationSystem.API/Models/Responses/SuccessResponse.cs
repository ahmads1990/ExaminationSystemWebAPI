namespace ExaminationSystem.API.Models.Responses;

/// <summary>
/// Represents a successful API response.
/// </summary>
/// <typeparam name="T">The type of the data returned.</typeparam>
public class SuccessResponse<T> : ApiResponse<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SuccessResponse{T}"/> class.
    /// </summary>
    /// <param name="data">The success payload.</param>
    /// <param name="message">A success message.</param>
    public SuccessResponse(T data, string message = "Success")
    {
        Data = data;
        Success = true;
        Message = message;
        ErrorCode = ApiErrorCode.None;
    }
}