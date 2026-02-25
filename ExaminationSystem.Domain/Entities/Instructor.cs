namespace ExaminationSystem.Domain.Entities;

public class Instructor : BaseModel
{
    public required AppUser AppUser { get; set; }
    public string Bio { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public List<int> CourseIds { get; set; } = new List<int>();
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
