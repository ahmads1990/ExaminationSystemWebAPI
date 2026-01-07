namespace ExaminationSystem.API.Models.Requests.Courses;

public class ListCoursesRequest : BasePaginatedRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? CreditHours { get; set; }
    public int? InstructorID { get; set; }
}
