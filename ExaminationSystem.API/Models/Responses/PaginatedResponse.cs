namespace ExaminationSystem.API.Models.Responses;

public class PaginatedResponse<T>
{
    public int TotalCount { get; set; }
    public IEnumerable<T> Data { get; set; }

    public PaginatedResponse(IEnumerable<T> data, int totalCount)
    {
        Data = data;
        TotalCount = totalCount;
    }
}
