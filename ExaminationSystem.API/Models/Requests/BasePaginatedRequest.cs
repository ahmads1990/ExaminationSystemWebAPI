using ExaminationSystem.API.Common;

namespace ExaminationSystem.API.Models.Requests;

public class BasePaginatedRequest
{
    public int PageIndex { get; set; } = Constants.DefaultPageIndex;
    public int PageSize { get; set; } = Constants.DefaultPageSize;
    public string? OrderBy { get; set; }
    public SortingDirection SortDirection { get; set; } = SortingDirection.Ascending;
}
