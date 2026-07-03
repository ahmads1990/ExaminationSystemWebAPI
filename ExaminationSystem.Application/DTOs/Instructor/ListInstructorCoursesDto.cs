using ExaminationSystem.Application.DTOs.Courses;

namespace ExaminationSystem.Application.DTOs.Instructor;

/// <summary>
/// Data Transfer Object for listing and filtering instructor courses.
/// </summary>
public class ListInstructorCoursesDto : BasePaginatedDto
{
    /// <summary>
    /// The allowed sorting fields.
    /// </summary>
    public static readonly IReadOnlyList<string> AllowedSortFields =
        [nameof(CourseStatsDto.CourseName), nameof(CourseStatsDto.StudentCount), nameof(CourseStatsDto.ExamsCount), nameof(CourseStatsDto.CourseId)];

    /// <summary>
    /// Search filter for the course name.
    /// </summary>
    public string? CourseName { get; set; }
}
