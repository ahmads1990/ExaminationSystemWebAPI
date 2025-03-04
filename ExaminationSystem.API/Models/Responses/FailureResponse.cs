namespace ExaminationSystem.API.Models.Responses;

public class FailureResponse<T> : BaseResponse<T>
{
    public FailureResponse(ErrorCode errorCode, string message = "")
    {
        Data = default!;
        IsSuccess = false;
        Message = message;
        ErrorCode = errorCode;
    }
}
