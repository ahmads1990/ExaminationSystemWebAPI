using ExaminationSystemWebAPI.Helpers.Enums;

namespace ExaminationSystemWebAPI.ViewModels.Response;

public class FailureResponseViewModel<T> : ResponseViewModel<T>
{
    public FailureResponseViewModel(ErrorCode errorCode, string message = "")
    {
        Data = default!;
        IsSuccess = false;
        Message = message;
        ErrorCode = errorCode;
    }
}
