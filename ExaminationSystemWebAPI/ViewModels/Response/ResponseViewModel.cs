using ExaminationSystemWebAPI.Helpers.Enums;

namespace ExaminationSystemWebAPI.ViewModels.Response;

public class ResponseViewModel<T>
{
    public T Data { get; set; } = default!;
    public bool IsSuccess { get; set; } = true;
    public ErrorCode ErrorCode { get; set; }
    public string Message { get; set; } = string.Empty;
}