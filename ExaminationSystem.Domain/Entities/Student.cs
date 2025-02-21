namespace ExaminationSystem.Domain.Entities;

public class Student : BaseModel
{
    public byte Grade { get; set; }

    public int AppUserId { get; set; }
    public AppUser AppUser { get; set; } = default!;
    public ICollection<StudentCourses> StudentCourses { get; set; } = new List<StudentCourses>();
    public ICollection<StudentExamsAnswers> StudentExamsAnswers { get; set; } = new List<StudentExamsAnswers>();
}
