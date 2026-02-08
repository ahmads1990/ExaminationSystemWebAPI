using ExaminationSystem.API.Extensions;

namespace ExaminationSystem.API.Models.Responses;

public class ErrorResponse<T> : ApiResponse<T>
{
    public ErrorResponse(ApiErrorCode errorCode, string? customMessage = null)
    {
        Success = false;
        Data = default;
        ErrorCode = errorCode;
        Message = customMessage ?? errorCode.GetErrorMessage();
    }
}