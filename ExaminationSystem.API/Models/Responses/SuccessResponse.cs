namespace ExaminationSystem.API.Models.Responses;

public class SuccessResponse<T> : BaseResponse<T>
{
    public SuccessResponse(T data, string message = "")
    {
        Data = data;
        IsSuccess = true;
        Message = message;
        ErrorCode = ErrorCode.None;
    }
}
