using ExaminationSystemWebAPI.Models.Users;

namespace ExaminationSystemWebAPI.Models.Joins;

public class StudentCourses : BaseModel
{
    public DateTime EnrollmentDate { get; set; }
    public bool Finished { get; set; }

    public string StudentID { get; set; } = string.Empty;
    public Student Student { get; set; } = default!;

    public string CourseID { get; set; } = string.Empty;
    public Course Course { get; set; } = default!;
}
