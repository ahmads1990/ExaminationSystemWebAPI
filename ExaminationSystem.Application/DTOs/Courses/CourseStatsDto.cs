namespace ExaminationSystem.Application.DTOs.Courses;

/// <summary>
/// Data Transfer Object representing course statistics for the instructor dashboard.
/// </summary>
public class CourseStatsDto
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public int ExamsCount { get; set; }
}
