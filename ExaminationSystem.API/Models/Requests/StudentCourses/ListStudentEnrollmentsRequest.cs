namespace ExaminationSystem.API.Models.Requests.StudentCourses;

public class ListStudentEnrollmentsRequest
{
    public string CourseTitle { get; set; } = string.Empty;
    public bool OnlyEnrolled { get; set; }
}
