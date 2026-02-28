using ExaminationSystem.Domain.Entities;

namespace ExaminationSystem.Application.DTOs.Courses;

public class ListCoursesDto : BasePaginatedDto
{
    public static readonly IReadOnlyList<string> AllowedSortFields =
        [nameof(Course.InstructorID), nameof(Course.CreditHours), nameof(Course.CreatedDate), nameof(Course.ID)];

    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? CreditHours { get; set; }
    public int? InstructorID { get; set; }
}
