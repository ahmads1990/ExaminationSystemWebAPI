namespace ExaminationSystem.Domain.Entities;

public class StudentCourses : BaseModel
{
    public DateTime EnrollmentDate { get; set; }
    public bool Finished { get; set; }

    public int StudentID { get; set; }
    public required Student Student { get; set; }

    public int CourseID { get; set; }
    public required Course Course { get; set; }
}
