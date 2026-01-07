namespace ExaminationSystem.Application.DTOs.Courses;

public class ListCoursesDto : BasePaginatedDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? CreditHours { get; set; }
    public int? InstructorID { get; set; }
}
