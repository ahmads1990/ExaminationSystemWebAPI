namespace ExaminationSystem.Application.DTOs.Courses;

public class CourseDto
{
    public int ID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CreditHours { get; set; }
    public int InstructorID { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}
