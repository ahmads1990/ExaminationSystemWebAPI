namespace ExaminationSystem.Domain.Entities;

public class StudentCourses : BaseModel
{
    public DateTime EnrollmentDate { get; set; }
    public bool Finished { get; set; }

    public int StudentID { get; set; }
    public Student Student { get; set; } = default!;

    public int CourseID { get; set; }
    public Course Course { get; set; } = default!;
}
