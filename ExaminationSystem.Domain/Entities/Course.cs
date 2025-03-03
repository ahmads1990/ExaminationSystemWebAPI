namespace ExaminationSystem.Domain.Entities;

public class Course : BaseModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CreditHours { get; set; }

    public int InstructorID { get; set; }
    public required Instructor Instructor { get; set; }
    public ICollection<StudentCourses> StudentCourses { get; set; } = new List<StudentCourses>();
}
