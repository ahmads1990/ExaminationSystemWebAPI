using ExaminationSystemWebAPI.Helpers.Enums;

namespace ExaminationSystemWebAPI.ViewModels.Response;

public class SuccessResponseViewModel<T> : ResponseViewModel<T>
{
    public SuccessResponseViewModel(T data, string message = "")
    {
        Data = data;
        IsSuccess = true;
        Message = message;
        ErrorCode = ErrorCode.None;
    }
}
