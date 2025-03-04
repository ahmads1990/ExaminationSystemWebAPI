namespace ExaminationSystem.API.Models.Responses;

public class BaseResponse<T>
{
    public T Data { get; set; }
    public bool IsSuccess { get; set; } = true;
    public ErrorCode ErrorCode { get; set; }
    public string Message { get; set; } = string.Empty;
}
