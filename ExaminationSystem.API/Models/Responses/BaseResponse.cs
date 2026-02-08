namespace ExaminationSystem.API.Models.Responses;

public abstract class ApiResponse<T>
{
    public bool Success { get; set; } = false;
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public ApiErrorCode ErrorCode { get; set; } = ApiErrorCode.None;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}