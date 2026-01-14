namespace ExaminationSystem.Application.DTOs.StudentCourses;

public class StudentEnrollmentDto
{
    public int CourseID { get; set; }
    public string CourseName { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public bool Finished { get; set; }
}
