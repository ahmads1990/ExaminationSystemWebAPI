namespace ExaminationSystem.Domain.Entities;

public class Student : BaseModel
{
    public required AppUser AppUser { get; set; }

    public string Level { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;

    public ICollection<StudentCourses> StudentCourses { get; set; } = new List<StudentCourses>();
    public ICollection<StudentExamsAnswers> StudentExamsAnswers { get; set; } = new List<StudentExamsAnswers>();
    public ICollection<ExamAttempt> ExamAttempts { get; set; } = new List<ExamAttempt>();
}
