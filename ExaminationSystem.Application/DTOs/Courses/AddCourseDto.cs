namespace ExaminationSystem.Application.DTOs.Courses;

public class AddCourseDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CreditHours { get; set; }
    public int InstructorID { get; set; }
}
