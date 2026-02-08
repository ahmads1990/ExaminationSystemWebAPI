namespace ExaminationSystem.API.Models.Responses;

public class SuccessResponse<T> : ApiResponse<T>
{
    public SuccessResponse(T data, string message = "Success")
    {
        Data = data;
        Success = true;
        Message = message;
        ErrorCode = ApiErrorCode.None;
    }
}