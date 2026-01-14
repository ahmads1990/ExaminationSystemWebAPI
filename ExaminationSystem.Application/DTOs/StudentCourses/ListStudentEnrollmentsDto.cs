namespace ExaminationSystem.Application.DTOs.StudentCourses;

public class ListStudentEnrollmentsDto : BasePaginatedDto
{
    public int StudentId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
}
