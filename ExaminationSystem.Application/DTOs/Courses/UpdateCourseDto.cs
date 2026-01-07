namespace ExaminationSystem.Application.DTOs.Courses;

public class UpdateCourseDto
{
    public int ID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CreditHours { get; set; }
}
