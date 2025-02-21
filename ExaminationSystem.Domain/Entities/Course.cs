namespace ExaminationSystem.Domain.Entities;

public class Course : BaseModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CreditHours { get; set; }

    public int InstructorID { get; set; }
    public Instructor Instructor { get; set; } = default!;
    public ICollection<StudentCourses> StudentCourses { get; set; } = new List<StudentCourses>();
}
