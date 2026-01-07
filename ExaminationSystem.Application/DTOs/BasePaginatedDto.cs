namespace ExaminationSystem.Application.DTOs;

public class BasePaginatedDto
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public string? OrderBy { get; set; }
    public SortingDirection SortDirection { get; set; } = SortingDirection.Ascending;
}
